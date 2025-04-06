window.showToastr = function (type, message) {
    if (type == "success") {

        toastr.success(message);
    }
    if (type == "error") {
        toastr.error(message);
    }
}

toastr.options = {
    "timeOut": "5000",
    "closeButton": true,
    "progressBar": true
};

window.ShowSwal = function (type, message) {
    return new Promise((test) => {
        if (type == "success") {
            Swal.fire({
                title: "คุณแน่ใจที่จะลบสินค้าหรือไม่?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: "ตกลง"
            }).then((result) => {
                if (result.isConfirmed) {
                    test(true);
                } else {
                    test(false);
                }
            });
        }
        else if (type == "error") {
            Swal.fire({
                title: "Task Failed!",
                text: message,
                icon: "error"
            }).then(() => {
                test(false);
            });
        }
        else if (type == "test") {
            Swal.fire({
                icon: "success",
                title: message,
                showConfirmButton: false,
                timer: 1500
            }).then(() => {
                test(false);
            });
        }
    });
};




