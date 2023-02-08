<script setup>
import { ref, defineProps, onMounted, inject } from "vue";
import * as vgca from "../../util/vgcaplugin";
import moment from "moment";
const swal = inject("$swal");
const basedomainURL = fileURL;
const axios = inject("axios"); // inject axios
const store = inject("store");
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  TypeSign: String,
  File: Object,
  returnNewPath: Function,
  DocObj: Object,
  key: Number,
});

const SignFileCallBack1 = (rv) => {
            var received_msg = JSON.parse(rv);
            console.log(received_msg);
            if (received_msg.Status == 0){
              props.File.file_path = received_msg.FileServer;
              props.returnNewPath(received_msg.FileServer);
            }
        }
const exc_sign_approved = () => {
  swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
            var prms = {};

            prms["FileUploadHandler"] = baseURL + "/ConfigCA/FileUploadHandler.aspx";
            prms["SessionId"] = "";
            prms["FileName"] = baseURL + props.File.file_path;

            var json_prms = JSON.stringify(prms);
            vgca.vgca_sign_approved(json_prms, SignFileCallBack1);
            swal.close();

}
const exc_sign_issued = () => {
  swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
            var prms = {};

            prms["FileUploadHandler"] = baseURL + "/ConfigCA/FileUploadHandler.aspx";
            prms["SessionId"] = "";
            prms["FileName"] = baseURL + props.File.file_path;
            prms["DocNumber"] = props.DocObj.doc_code;
            prms["IssuedDate"] = (moment(props.DocObj.doc_date, "DD/MM/YYYY").toDate()).toISOString();

            var json_prms = JSON.stringify(prms);
            vgca.vgca_sign_issued(json_prms, SignFileCallBack1);
            swal.close();
}
const exc_appendix = () => {
  swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
            var prms = {};
            var scv = [];

            prms["FileUploadHandler"] = baseURL + "/ConfigCA/FileUploadHandler.aspx";
            prms["SessionId"] = "";
            prms["FileName"] = baseURL + props.File.file_path;;
            prms["DocNumber"] = props.DocObj.doc_code;
            prms["MetaData"] = scv;

            var json_prms = JSON.stringify(prms);
            vgca.vgca_sign_appendix(json_prms, SignFileCallBack1);
            swal.close();
        }
const convert_to_pdf = () => {
  axios
      .post(
        baseURL + "/api/DocMain/ConvertToPDF",
        props.File,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          props.File.file_path = props.File.file_path.substr(0, props.File.file_path.lastIndexOf(".")) + ".pdf";
          click_CA();
        } else {
          swal.fire({
            title: "Thông báo!",
            text:
              "Định dạng file không hỗ trợ!",
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
}
const click_CA = () =>{
  if(props.File.file_path.split('.').pop().toLowerCase() !== "pdf"){
    swal
    .fire({
      title: "Thông báo",
      text: "Để sử dụng chứng thư số file phải có định dạng pdf! Bạn có muốn chuyển đổi file sang định dạng pdf không?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        convert_to_pdf();
      }
    });
    return false;
  }
  switch(props.TypeSign){
    case 'sign':
    exc_sign_approved();
    break;
    case 'stamp':
      if(props.File.is_type === 0)
    exc_sign_issued();
    else if(props.File.is_type === 1)
    exc_appendix();
    break;
  }
}
onMounted(() => {
  return {
  };
});
</script>
<template>
  <Button @click="click_CA" label="Sử dụng chứng thư số" class="p-button-raised" />
</template>
<style lang="scss" scoped>
.p-chip{
    border-radius: 5px;
}
</style>
