function initDataTable(tableId, options = {}) {

    const defaultOptions = {
        responsive: true,
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        deferRender: true,

        dom:
            "<'dt-toolbar row mb-3'<'col-md-6 d-flex align-items-center gap-2'l><'col-md-6 d-flex justify-content-end'f>>" +
            "<'row'<'col-12'tr>>" +
            "<'dt-footer row mt-3'<'col-md-6'i><'col-md-6 d-flex justify-content-end'p>>",

        language: {
            search: "",
            searchPlaceholder: "Search anything...",
            lengthMenu: "_MENU_",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            paginate: {
                previous: "‹",
                next: "›"
            }
        },

        initComplete: function () {
            $(tableId).css('opacity', '1');
        }
    };

    const finalOptions = $.extend(true, {}, defaultOptions, options);

    $(tableId).css('opacity', '0');

    return $(tableId).DataTable(finalOptions);
}

