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
  type: 1,
});
let tempHTML = "";
let dochtml = "";
const isHasHTML = ref(false);
const spans = ref([]);
const displayTable = ref(false);

//Function handle
const initDocHTML = (html) => {
  html = html.replaceAll("&nbsp;", " ");
  html = html.replace(/[\.…]{2,}/gim, "{{}}");
  dochtml = document.getElementById("dochtml");
  dochtml.innerHTML = html;
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
  formData.append("type", options.value.type || null);
  //formData.append("file", event.files[0]);
  try {
    axios
      .post(api + "api/tool/genderHtml", formData, config)
      .then((response) => {
        debugger;
        var data = response.data;
        if (data) {
          tempHTML = "";
          initDocHTML(data.html);
        }
      });
    swal.close();
    if (options.value.loading) options.value.loading = false;
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
  if (route.params && Object.entries(route.params).length > 0) {
    options.value.profile_id = route.params.id;
    options.value.type = 1;
    uploadMyFile();
  }
});
</script>
<template>
  <div spellcheck="false" id="dochtml">test</div>
</template>
<style scoped></style>
