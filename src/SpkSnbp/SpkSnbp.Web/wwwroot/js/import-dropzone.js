$(function () {
    $('.import-file-input').each(function () {
        const fileInput = this;
        const label = $(fileInput).siblings('label[for="' + $(fileInput).attr('id') + '"]').get(0);

        window.addEventListener("drop", (e) => {
            if ([...e.dataTransfer.items].some((item) => item.kind === "file")) {
                e.preventDefault();
            }
        });

        label.addEventListener("dragover", (e) => {
            const fileItems = [...e.dataTransfer.items].filter(
                (item) => item.kind === "file",
            );

            if (fileItems.length > 0) {
                e.preventDefault();
                e.dataTransfer.dropEffect = "copy";
            }
        });

        window.addEventListener("dragover", (e) => {
            const fileItems = [...e.dataTransfer.items].filter(
                (item) => item.kind === "file",
            );
            if (fileItems.length > 0) {
                e.preventDefault();
                if (!label.contains(e.target)) {
                    e.dataTransfer.dropEffect = "none";
                }
            }
        });

        label.addEventListener("drop", function (e) {
            const fileItems = [...e.dataTransfer.items].filter(
                (item) => item.kind === "file",
            );
            if (fileItems.length > 0) {
                e.preventDefault();

                const fileItem = fileItems[0].getAsFile();
                const dataTransfer = new DataTransfer();
                dataTransfer.items.add(fileItem);
                fileInput.files = dataTransfer.files;

                const orgStr = $(label).children('.file-name').text();
                const closeBtn = $(' <button class="btn"><i class="fas fa-times text-danger"></i></div></button>');
                $(closeBtn).on('click', function (e) {
                    e.stopPropagation();
                    const emptyDataTransfer = new DataTransfer();
                    fileInput.files = emptyDataTransfer.files;
                    $(label).children('.file-name').text(orgStr);
                })

                $(label).children('.file-name').text(fileItem.name)
                $(label).children('.file-name').append(closeBtn);
            }
        });

        $(fileInput).on('change', function () {
            const orgStr = $(label).children('.file-name').text();
            const closeBtn = $(' <button class="btn"><i class="fas fa-times text-danger"></i></div></button>');
            $(closeBtn).on('click', function (e) {
                e.stopPropagation();
                const emptyDataTransfer = new DataTransfer();
                fileInput.files = emptyDataTransfer.files;
                $(label).children('.file-name').text(orgStr);
            })

            $(label).children('.file-name').text(fileInput.files[0].name)
            $(label).children('.file-name').append(closeBtn);
        })
    })
});