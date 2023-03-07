<script setup>
import { ref, inject, onMounted, computed } from "vue";
import { useRoute } from "vue-router";

const route = useRoute();
const router = inject("router");
const cryoptojs = inject("cryptojs");
const store = inject("store");
const axios = inject("axios");
const swal = inject("$swal");
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const api = "http://10.211.55.20:6868/";

//Declare
const options = ref({
  loading: false,
  profile_id: null,
  type: null,
});
let tempHTML = "";
let dochtml = "";
const isHasHTML = ref(false);
const spans = ref([]);
const displayTable = ref(false);

//Function handle
const genKey = () => {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substring(2, 8);
    return s ? "-" + p.substring(0, 4) + "-" + p.substring(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
};

const initDocHTML = (html) => {
  html = html.replaceAll("&nbsp;", " ");
  html = html.replace(/[\.…]{2,}/gim, "{{}}");
  dochtml = document.getElementById("dochtml");
  // iframe.innerHTML= html;
  dochtml.contentWindow.document.open();
  dochtml.contentWindow.document.write(html);
  dochtml.contentWindow.document.close();
  dochtml.contentWindow.document.querySelector("body").style.cssText =
    "background: #eee; padding: 20px; margin: 0;";
  dochtml.contentWindow.document.querySelector("div").style.cssText =
    "background: #fff; padding: 20mm;";
  isHasHTML.value = true;
  spans.value = [];
  eventDoc();
};
const eventDoc = () => {
  dochtml.querySelectorAll("table").forEach((element) => {
    element.addEventListener("click", function (e) {
      tableElement = element;
      displayTable.value = true;
      e.stopPropagation();
    });
  });
  dochtml.querySelectorAll("td").forEach((element) => {
    element.style.setProperty("vertical-align", "middle");
    element.style.setProperty("align-items", "center");
  });
  let groups = [];
  dochtml.querySelectorAll("p>span").forEach((element) => {
    element.setAttribute("contenteditable", true);
    if (
      element.closest("td") &&
      element.closest("td").querySelectorAll("span").length == 1
    ) {
      element.classList.add("maxw");
    }
    let hasedid = false;
    if (element.className && element.className.match(/os-span-edit-os/)) {
      hasedid = true;
      element.id = element.className.split("os-span-edit-os")[0];
      element.className = element.className.replace(
        "os-span-edit-os",
        " os-span-edit-os"
      );
    }
    const clone = element.cloneNode(true);
    let osid = uid();
    if (!clone.id) {
      clone.id = osid;
    } else if (hasedid) {
      osid = clone.id;
    }
    spans.value.push({
      element: clone.outerHTML,
      id: osid,
      version: 1,
      historys: [],
    });
    if (!element.id) {
      element.id = osid;
      if (!hasedid) element.classList.add(osid);
    }
    element.addEventListener("click", addClickSpan);
    element.addEventListener("blur", addBlurSpan);
    //Check group
    let group = element.className.split("os-span-edit-osgroup-");
    if (group.length > 1) {
      let groupdataclass = group[1].trim();
      if (groups.indexOf(groupdataclass) == -1) {
        groups.push(groupdataclass);
      }
      element.parentElement.classList.add(groupdataclass);
    }
  });
  //set Groups
  groups.forEach((gr) => {
    let wrapper = document.querySelector(".for-data" + gr);
    if (wrapper == null) {
      wrapper = document.createElement("div");
      wrapper.classList.add("for-data" + gr);
      dochtml.querySelectorAll("p." + gr).forEach((p) => {
        wrap(p, wrapper);
      });
    }
    wrapper.addEventListener("click", function (e) {
      tableElement = wrapper;
      displayTable.value = true;
      e.stopPropagation();
    });
  });
};

//Function onClick
const uploadMyFile = async (event) => {
  if (options.value.loading) {
    return;
  }
  options.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  formData.append("file", event.files[0]);
  try {
    const response = await axios.post(
      api + "api/tool/genderHtml",
      formData,
      config
    );
    if (response) {
      var data = response.data;
      if (data) {
        tempHTML = "";
        initDocHTML(data.html);
        options.value.loading = false;
      }
    }
    swal.close();
    options.value = false;
  } catch (e) {
    swal.close();
    console.log(e);
  }
};

//init
onMounted(() => {
  // if (route.params.id != null) {
  //   options.value.profile_id = route.params.id;
  // } else {
  //   router.back();
  //   return;
  // }
  options.value.profile_id = "1";
  options.value.type = "1";
  var infoHtml = document.getElementById("info-html");
  var iframe = infoHtml.querySelector("iframe");

  iframe.setAttribute(
    "src",
    location.origin +
      "/hrm/iframe/" +
      options.value.profile_id +
      "/" +
      options.value.type +
      "?v=" +
      genKey()
  );
  //iframe.addEventListener("load", () => {});
});
</script>
<template>
  <div class="flex flex-grow-1 w-full h-full">
    <div class="layout-left h-full">
      <FileUpload
        chooseLabel="Chọn File Word"
        mode="basic"
        :auto="true"
        :customUpload="true"
        @uploader="uploadMyFile"
        name="doc[]"
        accept=".doc,.docx"
      />
    </div>
    <div id="info-html" class="layout-right w-full h-full">
      <div class="shadow-1 w-full h-full">
        <iframe frameborder="0" class="w-full h-full"></iframe>
      </div>
    </div>
  </div>
</template>
<style scoped>
.layout-left {
  max-width: calc(100vw - 88%) !important;
  min-width: 25rem !important;
  border: solid 1px rgba(0, 0, 0, 0.1);
  padding: 1rem;
}
.layout-right > div {
  padding: 20mm;
  display: block;
  page-break-before: always;
  background-color: #fff;
}

.layout-right {
  height: calc(100vh - 50px);
  overflow-y: auto;
  padding: 20px;
  background-color: rgb(204, 204, 204);
}
</style>
