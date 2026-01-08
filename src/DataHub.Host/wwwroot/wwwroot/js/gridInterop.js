let dotNetHelper;
let gridElement; // New variable to store the grid element

function gridKeydownHandler(e) {
    console.log("gridKeydownHandler: Key pressed:", e.key, "Target:", e.target);

    // Ensure Blazor.platform is available before trying to use it
    if (typeof Blazor === 'undefined' || typeof Blazor.platform === 'undefined') {
        console.warn("Blazor.platform is not yet available. Skipping gridKeydownHandler logic.");
        return;
    }

    // Resolve the Blazor ElementReference to the actual DOM element
    const actualGridElement = Blazor.platform.findJSObjectById(gridElement.__jsObjectId);

    // Check if the event target is within our specific gridElement
    if (!actualGridElement || !actualGridElement.contains(e.target)) {
        return; // Not our grid, ignore
    }

    // Existing Enter, PageDown, PageUp logic
    if (e.key === 'Enter') {
        e.preventDefault();
        dotNetHelper.invokeMethodAsync('HandleEnterKey');
    } else if (e.key === 'PageDown') {
        e.preventDefault();
        dotNetHelper.invokeMethodAsync('HandlePageDownKey');
    } else if (e.key === 'PageUp') {
        e.preventDefault();
        dotNetHelper.invokeMethodAsync('HandlePageUpKey');
    }
    // Add logic for ArrowUp/Down/Left/Right to find and focus the text field
    else if (e.key === 'ArrowUp' || e.key === 'ArrowDown' || e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
        e.preventDefault(); // Prevent default scrolling

        // Find the currently active row
        const activeRow = gridElement.querySelector('.fluent-data-grid__row--active');

        if (activeRow) {
            const fluentTextField = activeRow.querySelector('fluent-text-field');
            if (fluentTextField) {
                const inputElement = fluentTextField.shadowRoot ? fluentTextField.shadowRoot.querySelector('input') : fluentTextField.querySelector('input');
                if (inputElement) {
                    inputElement.focus();
                    inputElement.select();
                    console.log(`Focused input in active row for key: ${e.key}`);
                } else {
                    console.log("Input element not found inside fluent-text-field.");
                }
            } else {
                console.log("fluent-text-field not found in active row.");
            }
        } else {
            console.log("No active row found in the grid.");
        }
    }
}

window.initializeGridKeyboardListeners = (helper, element) => { // 'element' is now gridElement
    dotNetHelper = helper;
    gridElement = element; // Store the grid element globally in this script
    console.log("Attaching keydown listener to document, filtering for grid element:", gridElement); // NEW LOG
    document.addEventListener('keydown', gridKeydownHandler); // Attach to document
    console.log("Grid keyboard listeners initialized on document.");
};

window.disposeGridKeyboardListeners = () => {
    document.removeEventListener('keydown', gridKeydownHandler); // Remove from document
    dotNetHelper = null;
    gridElement = null; // Clear the reference
    console.log("Grid keyboard listeners disposed.");
};


window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
