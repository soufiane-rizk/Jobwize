window.jobWize = window.jobWize || {};

window.jobWize.downloadFile = (fileName, contentType, base64Content) => {
    const bytes = Uint8Array.from(atob(base64Content), character => character.charCodeAt(0));
    const url = URL.createObjectURL(new Blob([bytes], { type: contentType }));
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
};
