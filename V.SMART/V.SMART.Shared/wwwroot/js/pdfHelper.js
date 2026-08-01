
window.openPdfFromBase64 = function (base64Data) {
    try {
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);

        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: "application/pdf" });
        const url = URL.createObjectURL(blob);

        // Create or get overlay container
        let container = document.getElementById("pdfViewerContainer");
        if (!container) {
            container = document.createElement("div");
            container.id = "pdfViewerContainer";
            Object.assign(container.style, {
                position: "fixed",
                top: 0,
                left: 0,
                width: "100%",
                height: "100vh",
                backgroundColor: "rgba(0,0,0,0.8)",
                zIndex: 9999,
                display: "flex",
                justifyContent: "center",
                alignItems: "center",
                flexDirection: "column"
            });

            // Add a close button
            const closeBtn = document.createElement("button");
            closeBtn.innerText = "Close PDF";
            Object.assign(closeBtn.style, {
                position: "absolute",
                top: "15px",
                right: "15px",
                padding: "10px 15px",
                fontSize: "16px",
                cursor: "pointer",
                zIndex: 10000,
                backgroundColor: "#f44336",
                color: "white",
                border: "none",
                borderRadius: "4px",
            });

            closeBtn.onclick = () => {
                URL.revokeObjectURL(url);
                container.remove();
            };

            container.appendChild(closeBtn);
            document.body.appendChild(container);
        }

        // Create or get iframe inside container
        let iframe = document.getElementById("pdfViewer");
        if (!iframe) {
            iframe = document.createElement("iframe");
            iframe.id = "pdfViewer";
            Object.assign(iframe.style, {
                width: "80%",
                height: "90%",
                border: "none",
                boxShadow: "0 0 10px rgba(0,0,0,0.5)",
                borderRadius: "4px"
            });
            container.appendChild(iframe);
        }

        iframe.src = url;

    } catch (error) {
        console.error("Error displaying PDF:", error);
    }
};


