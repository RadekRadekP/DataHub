function downloadFileFromStream(fileName, contentStreamReference) {
    const link = document.createElement('a');
    link.href = URL.createObjectURL(contentStreamReference);
    link.download = fileName ?? '';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}