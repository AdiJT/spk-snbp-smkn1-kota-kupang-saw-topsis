"use strict";

$("#swal-1").click(function () {
  swal('Hello');
});

$("#swal-2").click(function () {
  swal('Good Job', 'You clicked the button!', 'success');
});

$("#swal-3").click(function () {
  swal('Good Job', 'You clicked the button!', 'warning');
});

$("#swal-4").click(function () {
  swal('Good Job', 'You clicked the button!', 'info');
});

$("#swal-5").click(function () {
  swal('Good Job', 'You clicked the button!', 'error');
});

$("#swal-6").click(function () {
  swal({
    title: 'Are you sure?',
    text: 'Once deleted, you will not be able to recover this imaginary file!',
    icon: 'warning',
    buttons: true,
    dangerMode: true,
  })
    .then((willDelete) => {
      if (willDelete) {
        swal('Poof! Your imaginary file has been deleted!', {
          icon: 'success',
        });
      } else {
        swal('Your imaginary file is safe!');
      }
    });
});

$("#swal-7").click(function () {
  swal({
    title: 'What is your name?',
    content: {
      element: 'input',
      attributes: {
        placeholder: 'Type your name',
        type: 'text',
      },
    },
  }).then((data) => {
    swal('Hello, ' + data + '!');
  });
});

$("#swal-8").click(function () {
  swal('This modal will disappear soon!', {
    buttons: false,
    timer: 3000,
  });
});

$("#swal-9").click(function () {
    swal({
        title: 'Anda yakin?',
        text: 'Once deleted, you will not be able to recover this imaginary file!',
        icon: 'warning',
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                form.submit(); // submit form ke action Hapus
                swal('Data berhasil dihapus', {
                    icon: 'success',
                });
            } else {
                
            }
        });
});

$(document).on("click", ".swal-confirm", function (e) {
    e.preventDefault();

    let form = $(this).closest("form");
    let url = form.attr("action");
    let data = form.serialize();

    swal({
        title: 'Anda yakin?',
        text: 'Data akan dihapus dan tidak bisa dikembalikan!',
        icon: 'warning',
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            $(form).trigger('submit');
            //$.post(url, data) // kirim POST ke action Hapus
            //    .done(function () {
            //        swal({
            //            title: "Data berhasil dihapus",
            //            icon: "success",                       
            //            timer: 2000
            //        }).then(() => {
            //            location.reload(); // reload setelah swal selesai
            //        });
            //    })
            //    .fail(function () {
            //        swal({
            //            title: "Gagal menghapus data",
            //            icon: "error",
            //            buttons: false,
            //            timer: 3000
            //        });
            //    });
        }
    });
});

$(document).on("click", ".swal-cancel", function (e) {
    e.preventDefault();

    swal({
        title: 'Yakin batal?',
        text: 'Perubahan tidak akan disimpan!',
        icon: 'warning',
        buttons: true,
        dangerMode: true,
    }).then((willCancel) => {
        if (willCancel) {
            history.back(); // kembali ke halaman sebelumnya
            // atau kalau mau ke Index:
            // window.location.href = '/Divisi/Index';
        }
    });
});
                   