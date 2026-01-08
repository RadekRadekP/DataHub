// This function is used for downloading files directly from a .NET StreamReference.
// It's more efficient for larger files as it avoids Base64 encoding overhead.
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? ''; // Use fileName if provided, otherwise empty string
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url); // Clean up the object URL to free memory
};

// Function to invoke a .NET method from JavaScript
window.blazorInterop = {
    invokeDotNetMethod: (dotNetHelper, methodName, arg) => {
        dotNetHelper.invokeMethodAsync(methodName, arg);
    }
};

// If you have other custom JS functions like 'saveAsFile' (for Base64 downloads),
// you can also define them in this file.

window.setDragData = (dataTransfer, format, data) => {
    dataTransfer.setData(format, data);
};

window.getDragData = (dataTransfer, format) => {
    return dataTransfer.getData(format);
};

window.setDropEffect = (dataTransfer, effect) => {
    dataTransfer.dropEffect = effect;
};

window.insertTextAtCursor = (elementId, text) => {
    console.log(`insertTextAtCursor: Called for elementId='${elementId}', text='${text}'`); // Added log
    const textArea = document.getElementById(elementId);
    if (!textArea) {
        console.error(`insertTextAtCursor: TextArea with ID '${elementId}' not found.`); // Added error log
        return;
    }

    const start = textArea.selectionStart;
    const end = textArea.selectionEnd;
    const value = textArea.value;

    textArea.value = value.substring(0, start) + text + value.substring(end);
    textArea.selectionStart = textArea.selectionEnd = start + text.length;
    textArea.focus();
    
    // Create and dispatch an 'input' event to notify Blazor of the change
    const event = new Event('input', { bubbles: true, cancelable: true });
    textArea.dispatchEvent(event);

    console.log(`insertTextAtCursor: Text inserted. New value: '${textArea.value}'`); // Added log
};