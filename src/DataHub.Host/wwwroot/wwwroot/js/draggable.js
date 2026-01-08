window.makeDraggable = function(element, handle) {
    let pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
    let isDragging = false;

    const dragHandle = handle || element;

    dragHandle.onmousedown = dragMouseDown;

    function dragMouseDown(e) {
        e = e || window.event;

        // Prevent dragging from starting on interactive elements within the handle
        const targetTagName = e.target.tagName.toUpperCase();
        const isInteractive = ['INPUT', 'TEXTAREA', 'BUTTON', 'SELECT', 'OPTION'].includes(targetTagName);
        
        const isFluentButton = e.target.closest('fluent-button');
        const isCloseButton = e.target.closest('.close-button');
        const isFluentTextField = e.target.closest('fluent-text-field');

        if (isInteractive || isFluentButton || isCloseButton || isFluentTextField) {
            // Do not start dragging on these elements
            return;
        }

        isDragging = true;
        e.preventDefault(); // Prevent text selection, etc.

        pos3 = e.clientX;
        pos4 = e.clientY;
        document.onmouseup = closeDragElement;
        document.onmousemove = elementDrag;
    }

    function elementDrag(e) {
        if (!isDragging) return;
        e = e || window.event;
        e.preventDefault();

        pos1 = pos3 - e.clientX;
        pos2 = pos4 - e.clientY;
        pos3 = e.clientX;
        pos4 = e.clientY;

        element.style.top = (element.offsetTop - pos2) + "px";
        element.style.left = (element.offsetLeft - pos1) + "px";
    }

    function closeDragElement() {
        isDragging = false;
        document.onmouseup = null;
        document.onmousemove = null;
    }
}