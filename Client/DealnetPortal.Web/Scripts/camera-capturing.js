$("#capture-canvas").hide();
$("#capture-buttons-2").hide();
var localMediaStream = null;
var video = document.querySelector('video');
var canvas = document.getElementById('capture-canvas');
var bigCanvas = document.getElementById('big-capture-canvas');

function takePhoto() {
    if (localMediaStream) {
        canvas.width = video.offsetWidth;
        canvas.height = video.offsetHeight;
        bigCanvas.height = video.videoHeight;
        bigCanvas.width = video.videoWidth;
        var ctx = bigCanvas.getContext('2d');
        ctx.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);
        ctx = canvas.getContext('2d');
        $("#capture-buttons-1").hide();
        ctx.drawImage(video, 0, 0, video.offsetWidth, video.offsetHeight);
        $("#video").hide();
        $("#capture-canvas").show();
        $("#capture-buttons-2").show();
    }
}

function retakePhoto() {
    $("#capture-canvas").hide();
    $("#capture-buttons-2").hide();
    $("#video").show();
    $("#capture-buttons-1").show();
}

$('#camera-modal').on('shown.bs.modal', function () {
    var constraints = {
        audio: false,
        video: {
            width: 9999,
            height: 9999,
            optional: [
                { minWidth: 320 },
                { minWidth: 640 },
                { minWidth: 800 },
                { minWidth: 900 },
                { minWidth: 1024 },
                { minWidth: 1280 },
                { minWidth: 1920 },
                { minWidth: 2560 }
            ]
        }
    };
    navigator.getUserMedia(constraints, function (stream) {
        video.src = window.URL.createObjectURL(stream);
        localMediaStream = stream;
    }, function (e) {
        console.log(e);
    });
});

$('#camera-modal').on('hidden.bs.modal', function () {
    if (video) {
        video.pause();
    }
    if (localMediaStream && localMediaStream.stop) {
        localMediaStream.stop();
    }
});

navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia;
window.URL = window.URL || window.webkitURL;