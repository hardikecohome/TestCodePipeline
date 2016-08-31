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

$('#camera-modal').on('shown.bs.modal', function() {
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
    navigator.getUserMedia(constraints, function(stream) {
        video.src = window.URL.createObjectURL(stream);
        localMediaStream = stream;
    }, function(e) {
        console.log(e);
    });
});

$('#camera-modal').on('hidden.bs.modal', function() {
    video.pause();
    localMediaStream.stop();
});

navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia;
window.URL = window.URL || window.webkitURL;

function uploadCaptured() {
    var dataUrl = bigCanvas.toDataURL();
    $.ajax({
        type: "POST",
        url: "/NewRental/TestScanPosting/",
        data: {
        imgBase64: dataUrl
        }
}).done(function(o) {
    window.location.href = "/NewRental/TestLicenseScanning";
});
}

function submitUpload() {
    var files = document.getElementById('upload-file').files;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }

            $.ajax({
                type: "POST",
                url: document.getElementById('upload-file').getAttribute("data-uploadUrl"),
                contentType: false,
                processData: false,
                data: data,
                success: function(json) {
                    if (json.isRedirect) {
                        window.location.href = json.redirectUrl;
                    }
                },
                error: function(xhr, status, p3) {
                    alert(xhr.responseText);
                }
            });
        } else {
            alert("Browser doesn't support HTML5 file upload!");
        }
    }
}