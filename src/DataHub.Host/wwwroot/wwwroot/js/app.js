window.setFocusToElement = (element) => {
    console.log("setFocusToElement called with element:", element);
    if (element) {
        element.focus();
        console.log("Element focused:", element);
    } else {
        console.log("Element is null or undefined, cannot focus.");
    }
};