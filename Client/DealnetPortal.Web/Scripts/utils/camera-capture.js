module.exports('camera-capture', function (require) {
    var localMediaStream = null;
    var video;
    var canvas;
    var bigCanvas;
    var takePhotoBtn;
    var submitBtns;

    var init = function () {
        video = document.querySelector('video');
        canvas = document.getElementById('capture-canvas');
        bigCanvas = document.getElementById('big-capture-canvas');

        if (!canvas.hasAttribute('style')) {
            canvas.setAttribute('style', 'display:none');
        }
        takePhotoBtn = document.getElementById('capture-btn');
        submitBtns = document.getElementById('capture-buttons-2');
        if (!submitBtns.hasAttribute('style')) {
            submitBtns.setAttribute('style', 'display:none');
        }

        $('#camera-modal').on('shown.bs.modal', function () {
            var constraints = {
                audio: false,
                video: {
                    width: {
                        min: 320,
                        ideal: 1280,
                        max: 1920
                    },
                    height: {
                        min: 180,
                        ideal: 720,
                        max: 1080
                    }
                }
            };
            try {
                launchVideoStreaming(constraints);
            } catch (err) {
                console.log(err);
                // constraints = {
                //     audio: false,
                //     video: {
                //         width: 9999,
                //         height: 9999
                //     }
                // };
                // launchVideoStreaming(constraints);
            }
        });

        $('#camera-modal').on('hidden.bs.modal', function () {
            if (video) {
                video.pause();
            }
            if (localMediaStream && localMediaStream.getTracks()[0]) {
                localMediaStream.getTracks()[0].stop();
            }
        });

        document.getElementById('retake').addEventListener('click', retakePhoto);
    };

    var takePhoto = function () {
        if (localMediaStream) {
            canvas.width = video.offsetWidth;
            canvas.height = video.offsetHeight;
            bigCanvas.height = video.videoHeight;
            bigCanvas.width = video.videoWidth;
            var ctx = bigCanvas.getContext('2d');
            ctx.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);
            ctx = canvas.getContext('2d');
            takePhotoBtn.setAttribute('style', 'display:none');
            ctx.drawImage(video, 0, 0, video.offsetWidth, video.offsetHeight);
            video.setAttribute('style', 'display:none');
            canvas.removeAttribute('style');
            submitBtns.removeAttribute('style');
            return bigCanvas.toDataURL();
        }
        return null;
    };

    var retakePhoto = function () {
        canvas.setAttribute('style', 'display:none');
        submitBtns.setAttribute('style', 'display:none');
        video.removeAttribute('style');
        takePhotoBtn.removeAttribute('style');
    };

    function launchVideoStreaming(constraints) {
        if (navigator.mediaDevices) {
            navigator.mediaDevices.getUserMedia(constraints)
                .then(function (stream) {
                    video.srcObject = stream;
                    localMediaStream = stream;
                }).catch(console.log);
        } else {
            navigator.getUserMedia(constraints, function (stream) {
                video.src = window.URL.createObjectURL(stream);
                localMediaStream = stream;
            }, function (e) {
                console.log(e);
            });
        }
    }

    return {
        init: init,
        takePhoto: takePhoto,
        retakePhoto: retakePhoto
    };
});