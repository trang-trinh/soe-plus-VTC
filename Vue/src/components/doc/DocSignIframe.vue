<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import { encr } from "../../util/function";
import DocSignCA from "./DocSignCA.vue";
const cryoptojs = inject("cryptojs");
const props = defineProps({
    DocObj: Object,
    listFileUploaded: Array,
    Type: String,
    returnWatermark: Function
});
const axios = inject("axios"); // inject axios
const emitter = inject("emitter");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = fileURL;
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
// emitter
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "getWatermark":
      getWatermark();
      break;
    case "changeTypeSignature":
        if(obj.data !== null){
            changeSignature(obj.data);
        }
      break;
      case "changeDocCode":
        changeDocCode();
      break;
      case "changeDocDate":
        changeDocDate();
      break;
      case "changeDocSigner":
        changeDocSigner();
      break;
    default: break;
  }
});
// reload component
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
//
const DocObj = props.DocObj
const listFileUploaded = props.listFileUploaded
const stamps = ref([]);
var loadStamps = () => {
    axios
            .post(
                baseURL + "/api/DocProc/CallProc",
                {str: 
                    encr(JSON.stringify(
                    {
                        proc: "doc_ca_stamps_list_touse",
                        par: [
                            { par: "organization_id", va: store.getters.user.organization_id }
                        ],
                    }
                ),
                    SecretKey, cryoptojs)
                    .toString(),
                },
                config
            )
            .then((response) => {
                let data = JSON.parse(response.data.data)[0];
                if (data.length > 0) {
                    stamps.value = data;
                    initStampIframe();
                }
            })
            .catch((error) => {
                if (error && error.status === 401) {
                    swal.fire({
                        text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                        confirmButtonText: "OK",
                    });
                }
            });
};
const exists_files = ref([]);
var initStampIframe = () => {
    exists_files.value.push({ file_path: DocObj.file_path, file_name: DocObj.file_name,doc_master_id: DocObj.doc_master_id, is_type: 0, id: DocObj.doc_master_id });
    listFileUploaded.forEach(fi => {
        exists_files.value.push({ file_path: fi.file_path, file_name: fi.file_name,file_id: fi.file_id, is_type: 1, id: fi.file_id });
    });
    setTimeout(function () {
        exists_files.value.forEach(fi => {
            // set iframe
            document.getElementById('signiframe' + fi.id).onload = function () {
                let iframe = document.getElementById("signiframe" + fi.id);
                let innerDoc = iframe.contentWindow || iframe.contentDocument;
                let obj_postmessage = JSON.parse(JSON.stringify({
                   'titlesign': "1. Lấy số và đóng dấu",
                    'file': fi,
                    'docitem': DocObj,
                    'stamps': stamps.value,
                    'is_type': 'stamp'
                }))
                innerDoc.postMessage(obj_postmessage, "*");
                // innerDoc.getElementById('titlesign').innerHTML = "1. Lấy số và đóng dấu";
                // get thong tin va dau
                // renderInfoDoc('signiframe' + fi.id);
                // renderStamp(DocObj.is_stamp, 'signiframe' + fi.id);
                swal.close();
            };
            document.getElementById('signiframe' + fi.id).setAttribute("src", basedomainURL + '/PDFWriter/Viewer?url=' + fi.file_path);
            document.getElementById('signiframe' + fi.id).setAttribute("relsrc", fi.file_path);
        });
    }, 1000);
}
// Sign
const signatures = ref([]);
var loadSignatures = (type_signature) => {
    axios
            .post(
                baseURL + "/api/DocProc/CallProc",
                {str: 
                    encr(JSON.stringify(
                    {
                        proc: "doc_list_signatures",
                        par: [
                            { par: "user_key", va: store.getters.user.user_key }
                        ],
                    }
                ),
                    SecretKey, cryoptojs)
                    .toString()
                },
                config
            )
            .then((response) => {
                let data = JSON.parse(response.data.data)[0];
                if (data.length > 0) {
                    signatures.value = data[0];
                    initSignIframe(type_signature);
                }
            })
            .catch((error) => {
                if (error && error.status === 401) {
                    swal.fire({
                        text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                        confirmButtonText: "OK",
                    });
                }
            });
};
var initSignIframe = (type_signature) => {
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    // Type signature: 1 : Chu ky, 0 : Chu ky nhay
    exists_files.value.push({ flash_sign: type_signature === 1 ? false : true,doc_master_id: DocObj.doc_master_id, file_path: DocObj.file_path, file_name: DocObj.file_name, is_type: 0, id: DocObj.doc_master_id });
    listFileUploaded.forEach(fi => {
        exists_files.value.push({ flash_sign: type_signature === 1 ? false : true,file_id: fi.file_id, file_path: fi.file_path, file_name: fi.file_name, is_type: 1, id: fi.file_id });
    });
    setTimeout(function () {
        exists_files.value.forEach(fi => {
            // set iframe
            document.getElementById('signiframe' + fi.id).setAttribute("src", basedomainURL + '/PDFWriter/Viewer?url=' + fi.file_path);
            document.getElementById('signiframe' + fi.id).setAttribute("relsrc", fi.file_path);
            document.getElementById('signiframe' + fi.id).onload = function () {
                let iframe = document.getElementById("signiframe" + fi.id);
                let innerDoc = iframe.contentWindow || iframe.contentDocument;
                let obj_postmessage = JSON.parse(JSON.stringify({
                   'titlesign': "1. Chữ ký",
                    'file': fi,
                    'type_signature': type_signature,
                    'signatures': signatures.value,
                    'is_type': 'sign'
                }))
                innerDoc.postMessage(obj_postmessage, baseURL + "/PDFWriter/Viewer");
                swal.close();
            };
        });
    }, 1000);
}
var changeSignature = (type_signature) => {
    let fi = exists_files.value[activeIndex.value];
    fi.flash_sign = type_signature === 1 ? false : true;
    let iframe = document.getElementById("signiframe" + fi.id);
    let innerDoc = iframe.contentWindow || iframe.contentDocument;
    let obj_postmessage = JSON.parse(JSON.stringify({
        'titlesign': "1. Chữ ký và dấu",
        'file': fi,
        'type_signature': type_signature,
        'signatures': signatures.value,
        'is_type': 'sign'
    }))

    innerDoc.postMessage(obj_postmessage, "*");
    swal.close();
}
var changeDocCode = () => {
    exists_files.value.forEach(function(fi,idx) {
        let iframe = document.getElementById("signiframe" + fi.id);
        let innerDoc = iframe.contentWindow || iframe.contentDocument;
        let obj_postmessage = { "changedoccode": true, "docitem": JSON.parse(JSON.stringify(DocObj)) };
        innerDoc.postMessage(obj_postmessage, "*");
    });
}
var changeDocDate = () => {
    exists_files.value.forEach(function(fi,idx) {
        let iframe = document.getElementById("signiframe" + fi.id);
        let innerDoc = iframe.contentWindow || iframe.contentDocument;
        let obj_postmessage = { "changedocdate": true, "docitem": JSON.parse(JSON.stringify(DocObj)) };
        innerDoc.postMessage(obj_postmessage, "*");
    });
}
var changeDocSigner = () => {
    exists_files.value.forEach(function(fi,idx) {
        let iframe = document.getElementById("signiframe" + fi.id);
        let innerDoc = iframe.contentWindow || iframe.contentDocument;
        let obj_postmessage = { "changedocsigner": true, "docitem": JSON.parse(JSON.stringify(DocObj)) };
        innerDoc.postMessage(obj_postmessage, "*");
    });
}
var count_emit = 0;
var listenerWatermark = (evt) => {
    let wt_imgs = [];
    let wt_texts = [];
    count_emit++;
    if (evt.data.WaterImages && evt.data.WaterImages.length > 0) {
        // get image
        let imgs = evt.data.WaterImages.filter(x => x.ImageFile);
        // imgs = imgs.map(x => x.ImageFile = x.ImageRelPath);
        let obj_img = {};
        if (imgs.length > 0) {
            obj_img["" + evt.data.file_path] = imgs;
            wt_imgs.push(obj_img);
        }
        // get text
        let txts = evt.data.WaterImages.filter(x => x.Text);
        let obj_txt = {};
        if (txts.length > 0) {
            obj_txt["" + evt.data.file_path] = txts;
            wt_texts.push(obj_txt);
        }
    }
    if (count_emit === exists_files.value.filter(x => !x.is_ca_signed).length) {
        let sign_files = [];
        exists_files.value.forEach((fi) => {
            if (wt_imgs.find(x => Object.keys(x)[0] === fi.file_path) || wt_texts.find(x => Object.keys(x)[0] === fi.file_path)) {
                sign_files.push(fi);
            }
        });
        window.removeEventListener('message', listenerWatermark);
        let watermark_signed = { "wm_images": wt_imgs, "wm_texts": wt_texts, "sign_files": sign_files };
        returnCAFiles(watermark_signed);
    }
}
const returnCAFiles = (watermark_signed) => {
    let ca_files = [];
    exists_files.value.forEach((fi) => {
            if(fi.is_ca_signed){
                ca_files.push(fi);
            }
        });
        props.returnWatermark({watermark_obj: watermark_signed, 'ca_files': ca_files});
}
const getWatermark = () => {
    count_emit = 0;
    window.addEventListener("message", listenerWatermark);
    setTimeout(function () {
        exists_files.value.filter(x => !x.is_ca_signed).forEach(function(fi,idx) {
        let iframe = document.getElementById("signiframe" + fi.id);
        if(iframe){
            let innerDoc = iframe.contentWindow || iframe.contentDocument;
            let obj_postmessage = { "getwatermark": true, "file_path": fi.file_path };
            innerDoc.postMessage(obj_postmessage, "*");
        }
    })
    },1000);
    if(!exists_files.value.find(x=>!x.is_ca_signed)){
        window.removeEventListener('message', listenerWatermark);
        returnCAFiles({ "wm_images": [], "wm_texts": [], "sign_files": [] })
    }
}
const activeIndex = ref(0);

// CA Signed
const active_extension_file = ref();
const returnNewPath = (new_file_path) => {
    exists_files.value[activeIndex.value].file_path = new_file_path;
    exists_files.value[activeIndex.value].is_ca_signed = true;
}
const changeTabView = () => {
    forceRerender();
}
onMounted(() => {
    active_extension_file.value = DocObj.file_path.split('.').pop();
    if(props.Type === 'stamp'){
        loadStamps();
    }
    else if(props.Type === 'sign'){
        loadSignatures(1);
    }
    return {

    };
});
</script>
<template>
     <TabView @tab-change="changeTabView" v-model:activeIndex="activeIndex" scrollable>
        <TabPanel v-for="tab in exists_files" :key="tab.id" :header="tab.file_name">
            <div class="col-12 md:col-12" style="background-color: #fff;height: calc(100vh - 250px);">
                <iframe v-if="!tab.is_ca_signed" :id="'signiframe' + tab.id" style="height: 100%; width: 100%"></iframe>
                <iframe v-if="tab.is_ca_signed" :src="basedomainURL + '/Viewer?url=' + tab.file_path" class="w-full h-full"></iframe>
            </div>
	    </TabPanel>
    </TabView>
    <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
              <DocSignCA :key="componentKey" :returnNewPath="returnNewPath" :DocObj="DocObj" :File="exists_files[activeIndex]" :TypeSign="Type"/>        
          </div>
    </div>
</template>
<style lang="scss" scoped>
.p-chip {
    border-radius: 5px;
}
</style>
