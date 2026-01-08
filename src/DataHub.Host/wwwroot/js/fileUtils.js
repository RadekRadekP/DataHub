// wwwroot/js/fileUtils.js
function downloadFileFromBytes(fileName, byteBase64) {
    var link = document.createElement('a');
    link.download = fileName;
    link.href = "data:application/octet-stream;base64," + byteBase64;
    document.body.appendChild(link); // Required for Firefox
    link.click();
    document.body.removeChild(link);
}
