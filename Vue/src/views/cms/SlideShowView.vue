<script setup>
import { ref, inject, onMounted, computed } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { v4 as uuidv4 } from "uuid";
//init Model
const slideshow = ref({
  SlideShow_Name: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  SlideShow_Name: {
    required,
  },
};
const v$ = useVuelidate(rules, slideshow);
//Khai báo biến
const store = inject("store");
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  user_id: store.getters.user.user_id,
  Topic_ID: -1,
  Donvi_ID: store.getters.user.Donvi_ID,
  Lang_ID: store.getters.lang.Lang_ID,
  IsType: -1,
});
const tdTypes = [
  { value: 0, text: "SlideShow" },
  { value: 1, text: "Album ảnh" },
  { value: 2, text: "Video" },
  { value: 3, text: "Banner, Quảng cáo" },
  { value: 4, text: "Khác" },
];
function toObject(arr) {
  var rv = {};
  for (var i = 0; i < arr.length; ++i) rv[arr[i].value] = arr[i].text;
  return rv;
}
let objectTypes = toObject(tdTypes);
const slideshows = ref([]);
const displayAddSlideShow = ref(false);
const isFirst = ref(true);
const files = ref([]);
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportSlideShow("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportSlideShow("ExportExcelMau");
    },
  },
]);

//Khai báo function
function get_youtube_thumbnail(url, quality) {
  if (url) {
    var video_id, thumbnail, result;
    if ((result = url.match(/youtube\.com.*(\?v=|\/embed\/)(.{11})/))) {
      video_id = result.pop();
    } else if ((result = url.match(/youtu.be\/(.{11})/))) {
      video_id = result.pop();
    }

    if (video_id) {
      if (typeof quality == "undefined") {
        quality = "high";
      }

      var quality_key = "maxresdefault"; // Max quality
      if (quality == "low") {
        quality_key = "sddefault";
      } else if (quality == "medium") {
        quality_key = "mqdefault";
      } else if (quality == "high") {
        quality_key = "hqdefault";
      }

      var thumbnail =
        "http://img.youtube.com/vi/" + video_id + "/" + quality_key + ".jpg";
      return thumbnail;
    }
  }
  return false;
}
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.SlideShow_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.SlideShow_ID), 1);
};
const removeFile = (img) => {
  let idx = files.value.findIndex((x) => x.id == img.id);
  if (idx != -1) {
    files.value.splice(idx, 1);
  }
};
const handleFileUpload = (event, ia) => {
  if (ia) {
    files.value = [];
    var output = document.getElementById(ia);
    output.src = URL.createObjectURL(event.target.files[0]);
    output.onload = function () {
      URL.revokeObjectURL(output.src); // free memory
    };
  }
  var arrFiles = Array.from(event.target.files);
  files.value = files.value.concat(arrFiles);
  files.value
    .filter((x) => !x.id)
    .forEach((fi) => {
      fi.id = uuidv4();
      const reader = new FileReader();
      reader.readAsDataURL(fi);
      reader.onload = () => {
        let img = document.getElementById(fi.id);
        if (img) {
          img.src = reader.result;
        }
      };
    });
};
//Show Modal
const isAdd = ref(false);
const showModalAddSlideShow = () => {
  submitted.value = false;
  isAdd.value = true;
  slideshow.value = {
    SlideShow_ID: -1,
    SlideShow_Name: "",
    STT: slideshows.value.length + 1,
    Trangthai: true,
    Lang_ID: store.getters.lang.Lang_ID,
    Donvi_ID: store.getters.user.Donvi_ID
  };
  displayAddSlideShow.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddSlideShow = () => {
  displayAddSlideShow.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadSlideShow(true);
};
const onSearch = () => {
  if (opition.value.TopicTree) {
    let keys = Object.keys(opition.value.TopicTree);
    opition.value.Topic_ID = keys[0];
  }
  loadSlideShow(true);
};
const loadSlideShow = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_SlideShow_List",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Donvi_ID", va: opition.value.Donvi_ID },
          { par: "Topic_ID", va: opition.value.Topic_ID },
          { par: "Lang_ID", va: opition.value.Lang_ID },
          { par: "IsType", va: opition.value.IsType },
          { par: "s", va: opition.value.search },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        slideshows.value = data[0];
        opition.totalRecords = data[0].length;
      } else {
        slideshows.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const editSlideShow = (md) => {
  submitted.value = false;
  isAdd.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddSlideShow.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "CMS_SlideShow_Get", par: [{ par: "SlideShow_ID", va: md.SlideShow_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        if (data[0][0].Tukhoa) data[0][0].Tukhoa = data[0][0].Tukhoa.split(",");
        slideshow.value = data[0][0];
        selectTopic.value = {};
        selectTopic.value[
          slideshow.value.Topic_ID != null ? slideshow.value.Topic_ID : "-1"
        ] = true;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (selectTopic.value) {
    let keys = Object.keys(selectTopic.value);
    slideshow.value.Topic_ID = keys[0];
    if (slideshow.value.Topic_ID == -1) {
      slideshow.value.Topic_ID = null;
    }
  }
  addSlideShow();
};
const addSlideShow = () => {
  let formData = new FormData();
  var images = [];
  files.value.forEach((file) => {
    formData.append(file.name, file);
    images.push({ Image_Name: file.name });
  });
  let md = { ...slideshow.value };
  if (md.Tukhoa instanceof Array) {
    md.Tukhoa = md.Tukhoa.join(",");
  }
  formData.append("model", JSON.stringify(md));
  formData.append("images", JSON.stringify(images));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + "/api/SlideShow/Update_SlideShow",
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        files.value = [];
        toast.success("Cập nhật slideshow thành công!");
        loadSlideShow();
        closedisplayAddSlideShow();
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const delSlideShow = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá slideshow này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios
          .delete(baseURL + "/api/SlideShow/Del_SlideShow", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.SlideShow_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá slideshow thành công!");
              loadSlideShow();
              if (!md) selectedNodes.value = [];
            } else {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const upTrangthaiSlideShow = (md) => {
  let ids = [md.SlideShow_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/SlideShow/Update_TrangthaiSlideShow",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái slideshow thành công!");
        loadSlideShow();
        if (!md) selectedNodes.value = [];
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const exportSlideShow = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "CMS_SlideShow" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH SLIDESHOW",
        proc: "CMS_SlideShow_ListExport",
        par: par,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
            let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
            var listPath = pathReplace.split('/');
            var pathFile = "";
            listPath.forEach(item => {
              if (item.trim() != "")
              {
                  pathFile += "/" + item;
              }
            });
            //window.open(baseURL + response.data.path);
            window.open(baseURL + pathFile);
          }
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//Tudien Topic
const images = ref([]);
const treetopics = ref([]);
const selectTopic = ref();
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter(
      (x) => x.Parent_ID == null || data.findIndex((a) => a.Topic_ID == x.Parent_ID) == -1
    )
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.Parent_ID == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.Parent_ID == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const loadTopic = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_Topic_ListTudien",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Donvi_ID", va: opition.value.Donvi_ID },
          { par: "Lang_ID", va: opition.value.Lang_ID },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTree(data, "Topic_ID", "Topic_Name", "Biên mục");
        treetopics.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {});
};
//
const displayAddSlideShowImage = ref(false);
const displayAddSlideShowAllImage = ref(false);
const openAddSlideShowImage = ref(false);
const image = ref({});
const showModalAddSlideShowImage = () => {
  files.value = [];
  image.value = {
    Image_ID: -1,
    SlideShow_ID: slideshow.value.SlideShow_ID,
    STT: images.value.length + 1,
    Trangthai: true,
    IsVideo: slideshow.value.IsType == 2,
  };
  openAddSlideShowImage.value = true;
};
const editSlideShowImage = (md) => {
  files.value = [];
  image.value = { ...md };
  openAddSlideShowImage.value = true;
};
const openSlideShowImage = (sli, f) => {
  if (f != false) {
    slideshow.value = sli;
    images.value = [];
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddSlideShowImage.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_SlideShowImage_Get",
        par: [{ par: "SlideShow_ID", va: sli.SlideShow_ID }],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0]
          .filter((x) => x.IsFilePath)
          .forEach((img) => {
            let idx = img.IsFilePath.lastIndexOf(".");
            img.IsFilePathThumb =
              img.IsFilePath.substring(0, idx) + "_thumb" + img.IsFilePath.substring(idx);
          });
        images.value = data[0];
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const addSlideShowImage = () => {
  if (files.value.length == 0 && image.Image_ID == -1) {
    swal.fire({
      title: "Error!",
      text: "Vui lòng chọn ảnh slide!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let formData = new FormData();
  if (files.value.length > 0) {
    let file = files.value[0];
    formData.append(file.name, file);
  }
  formData.append("model", JSON.stringify(image.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + "/api/SlideShow/Add_SlideShowImage",
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật ảnh thành công!");
        openAddSlideShowImage.value = false;
        if (image.Image_ID == -1) slideshow.value.images += 1;
        files.value = [];
        openSlideShowImage(slideshow.value, false);
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const uploadAllImage = () => {
  let formData = new FormData();
  var images = [];
  files.value.forEach((file) => {
    formData.append(file.name, file);
    images.push({ Image_Name: file.name });
  });
  let md = { ...slideshow.value };
  if (md.Tukhoa instanceof Array) {
    md.Tukhoa = md.Tukhoa.join(",");
  }
  formData.append("model", JSON.stringify(md));
  formData.append("images", JSON.stringify(images));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + "/api/SlideShow/Update_SlideShowImageAll",
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật ảnh slideshow thành công!");
        openSlideShowImage(slideshow.value, false);
        displayAddSlideShowAllImage.value = false;
        slideshow.value.images += files.value.length;
        files.value = [];
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const delSlideShowImage = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá ảnh này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios
          .delete(baseURL + "/api/SlideShow/Del_SlideShowImage", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Image_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá ảnh thành công!");
              openSlideShowImage(slideshow.value, false);
              slideshow.value.images -= 1;
              if (!md) selectedNodes.value = [];
            } else {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const upTrangthaiSlideShowImage = (md) => {
  let ids = [md.Image_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/SlideShow/Update_TrangthaiSlideShowImage",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái ảnh thành công!");
        if (!md) selectedNodes.value = [];
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const rowClassActive = (data) => {
  return data.Trangthai ? "success" : "error";
};
const rowClassSlideActive = (data) => {
  return data.Trangthai ? "" : "error";
};
const compImages = computed(() => images.value.filter((x) => !x.IsVideo));
const compVideos = computed(() => images.value.filter((x) => x.IsVideo));
const showGalleria = ref(false);
const showVideo = ref(false);
const openModalVideo = () => {
  image.value = compVideos.value[0];
  showVideo.value = true;
};
const clickVideo=(item)=>{
  image.value=item;
}
//Emit lang
const emitter = inject("emitter");
emitter.on("lang", (obj) => {
  opition.value.Lang_ID = store.getters.lang.Lang_ID;
  loadSlideShow(true);
  loadTopic();
});
const changeLinkYoutube = () => {
  var thumbnail = get_youtube_thumbnail(image.valueis_link, "high");
  if (thumbnail != false) {
    var img = document.getElementById("LogoSlideImage");
    img.src = thumbnail;
    loadXHR(thumbnail).then(function (blob) {
      files.value = [blobToFile(blob, uuidv4() + ".jpg")];
      console.log(files.value);
    });
  }
};
function blobToFile(theBlob, fileName) {
  return new File([theBlob], fileName, {
    lastModified: new Date().getTime(),
    type: theBlob.type,
  });
}
function loadXHR(url) {
  return new Promise(function (resolve, reject) {
    try {
      var xhr = new XMLHttpRequest();
      xhr.open("GET", url);
      xhr.responseType = "blob";
      xhr.onerror = function () {
        reject("Network error.");
      };
      xhr.onload = function () {
        if (xhr.status === 200) {
          resolve(xhr.response);
        } else {
          reject("Loading error:" + xhr.statusText);
        }
      };
      xhr.send();
    } catch (err) {
      reject(err.message);
    }
  });
}
onMounted(() => {
  //init
  loadSlideShow(true);
  loadTopic();
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      :value="slideshows"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="lenient"
      class="p-datatable-sm e-sm"
      :paginator="opition.totalRecords > opition.PageSize"
      :rows="opition.PageSize"
      :pageLinkSize="opition.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      :row-class="rowClassSlideActive"
    >
      <template #header>
        <h3 class="slideshow-title mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Danh sách slideshow
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              class="mr-2"
              v-model="opition.IsType"
              :options="tdTypes"
              optionLabel="text"
              optionValue="value"
              placeholder="Chọn loại"
              @change="onSearch"
              :showClear="true"
            />
            <TreeSelect
              class="mr-2"
              @change="onSearch"
              v-model="opition.TopicTree"
              :options="treetopics"
              :showClear="true"
              placeholder="Chọn biên mục"
              optionLabel="data.Topic_Name"
              optionValue="data.Topic_ID"
            ></TreeSelect>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                v-model="opition.search"
                placeholder="Tìm kiếm"
                v-on:keyup.enter="onSearch"
              />
            </span>
          </template>
          <template #end>
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />
            <Button
              label="Xoá"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
              v-if="selectedNodes.length > 0"
              @click="delSlideShow"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu vị id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            <Button
              label="Thêm slideshow"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddSlideShow"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        :sortable="true"
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      ></Column>
      <Column field="SlideShow_Name" header="Tên slide" :sortable="true"> </Column>
      <Column
        :sortable="true"
        field="Topic_Name"
        header="Biên mục"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      ></Column>
      <Column
        :sortable="true"
        field="IsType"
        header="Loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px"
        bodyStyle="text-align:center;max-width:200px"
      >
        <template #body="md">
          <Chip :label="objectTypes[md.data.IsType]" :class="'chip' + md.data.IsType" />
        </template>
      </Column>
      <Column
        field="images"
        header="Ảnh"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:60px"
        bodyStyle="text-align:center;max-width:60px"
      >
        <template #body="md">
          <Badge
            @click="openSlideShowImage(md.data)"
            :value="md.data.images"
            severity="success"
          ></Badge>
        </template>
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:110px"
        bodyStyle="text-align:center;max-width:110px"
      >
        <template #body="md">
          <Checkbox
            v-model="md.data.Trangthai"
            :binary="true"
            @change="upTrangthaiSlideShow(md.data)"
          />
        </template>
      </Column>
      <Column
        headerClass="text-center"
        headerStyle="text-align:center;max-width:110px"
        bodyStyle="text-align:center;max-width:110px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editSlideShow(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delSlideShow(md.data)"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="m-auto align-items-center justify-content-center p-4 text-center"
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    header="Cập nhật slideshow"
    v-model:visible="displayAddSlideShow"
    :style="{ width: '720px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12" v-if="!isAdd">
          <label class="col-2 text-left">Mã</label>
          <InputText
            spellcheck="false"
            disabled="true"
            class="col-10 ip36"
            v-model="slideshow.SlideShow_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="slideshow.SlideShow_Name"
            :class="{ 'p-invalid': v$.SlideShow_Name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="
            (v$.SlideShow_Name.$invalid && submitted) ||
            v$.SlideShow_Name.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.SlideShow_Name.required.$message
                .replace("Value", "Tên slideshow")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại</label>
          <Dropdown
            class="col-10"
            v-model="slideshow.IsType"
            :options="tdTypes"
            optionLabel="text"
            optionValue="value"
          />
        </div>
        <div class="field col-12 md:col-12 flex" v-if="slideshow.SlideShow_ID == -1">
          <label class="col-2">Ảnh Slide</label>
          <div class="col-4 p-0">
            <div class="inputanh" @click="chonanh('AnhSlideShow')">
              <img id="LogoSlideShow" src="/src/assets/image/noimg.jpg" />
            </div>
            <input
              class="ipnone"
              id="AnhSlideShow"
              type="file"
              multiple
              accept="image/*"
              @change="handleFileUpload($event)"
            />
          </div>
        </div>

        <div class="field col-12 md:col-12" v-if="files.length > 0">
          <h3>Ảnh Slide</h3>
          <ul class="image-slide">
            <li v-for="img in files" :key="img.id">
              <img :id="img.id" width="100" />
              <Button
                @click="removeFile(img)"
                icon="pi pi-times"
                class="btremoimage p-button-rounded p-button-secondary"
              />
            </li>
          </ul>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Biên mục</label>
          <TreeSelect
            class="col-10"
            v-model="selectTopic"
            :options="treetopics"
            :showClear="true"
            placeholder=""
            optionLabel="data.Topic_Name"
            optionValue="data.Topic_ID"
          ></TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Từ khoá</label>
          <Chips
            spellcheck="false"
            class="col-10 p-0"
            v-model="slideshow.Tukhoa"
            :addOnBlur="true"
            separator=","
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="slideshow.STT" />
          <label class="col-2 text-right">Trạng thái</label>
          <InputSwitch
            v-model="slideshow.Trangthai"
            style="vertical-align: text-bottom"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddSlideShow"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog
    :header="'Slideshow ' + slideshow.SlideShow_Name"
    v-model:visible="displayAddSlideShowImage"
    :style="{ width: '1024px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="flex-grow-1">
      <DataTable
        :value="images"
        :filters="filters"
        :showGridlines="true"
        class="p-datatable-sm e-sm"
        :scrollable="true"
        :rowClass="rowClassActive"
        scrollHeight="flex"
      >
        <template #header>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <Button
                v-if="compImages.length > 0"
                label="Xem Slide"
                icon="pi pi-images"
                class="mr-2"
                @click="showGalleria = true"
              />
              <Button
                v-if="compVideos.length > 0"
                label="Xem Video"
                icon="pi pi-video"
                @click="openModalVideo"
              />
            </template>
            <template #end>
              <Button
                label="Thêm nhiều ảnh"
                icon="pi pi-plus"
                class="mr-2"
                @click="
                  displayAddSlideShowAllImage = true;
                  files = [];
                "
              />
              <Button
                :label="'Thêm ' + (slideshow.IsType == 2 ? 'Video' : 'Ảnh')"
                icon="pi pi-plus"
                class="mr-2"
                @click="showModalAddSlideShowImage"
              />
            </template>
          </Toolbar>
        </template>
        <Column
          :sortable="true"
          field="STT"
          header="STT"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:100px"
          bodyStyle="text-align:center;max-width:100px"
        ></Column>
        <Column
          field="Image_Name"
          header=""
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:80px"
          bodyStyle="text-align:center;max-width:80px"
        >
          <template #body="md">
            <img :src="basedomainURL + md.data.IsFilePath" width="64" height="36" />
          </template>
        </Column>
        <Column field="Image_Name" header="Tên ảnh" :sortable="true"> </Column>
        <Column
          field="Trangthai"
          header="Trạng thái"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:110px"
          bodyStyle="text-align:center;max-width:110px"
        >
          <template #body="md">
            <Checkbox
              v-model="md.data.Trangthai"
              :binary="true"
              @change="upTrangthaiSlideShowImage(md.data)"
            />
          </template>
        </Column>
        <Column
          headerClass="text-center"
          headerStyle="text-align:center;max-width:110px"
          bodyStyle="text-align:center;max-width:110px"
        >
          <template #header> </template>
          <template #body="md">
            <Button
              type="button"
              icon="pi pi-pencil"
              class="p-button-rounded p-button-sm p-button-info"
              style="margin-right: 0.5rem"
              @click="editSlideShowImage(md.data)"
            ></Button>
            <Button
              type="button"
              icon="pi pi-trash"
              class="p-button-rounded p-button-sm p-button-danger"
              @click="delSlideShowImage(md.data)"
            ></Button>
          </template>
        </Column>
        <template #empty>
          <div
            class="m-auto align-items-center justify-content-center p-4 text-center"
            v-if="!isFirst"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayAddSlideShowImage = false"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
  <Dialog
    :header="'Slideshow ' + slideshow.SlideShow_Name"
    v-model:visible="displayAddSlideShowAllImage"
    :style="{ width: '640px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="flex-grow-1">
      <div class="field col-12 md:col-12 flex">
        <label class="col-2">Ảnh Slide</label>
        <div class="col-4 p-0">
          <div class="inputanh" @click="chonanh('AnhSlideShow')">
            <img id="LogoSlideShow" src="/src/assets/image/noimg.jpg" />
          </div>
          <input
            class="ipnone"
            id="AnhSlideShow"
            type="file"
            multiple
            accept="image/*"
            @change="handleFileUpload($event)"
          />
        </div>
      </div>

      <div class="field col-12 md:col-12" v-if="files.length > 0">
        <h3>Ảnh Slide</h3>
        <ul class="image-slide">
          <li v-for="img in files" :key="img.id">
            <img :id="img.id" width="100" />
            <Button
              @click="removeFile(img)"
              icon="pi pi-times"
              class="btremoimage p-button-rounded p-button-secondary"
            />
          </li>
        </ul>
      </div>
    </div>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayAddSlideShowAllImage = false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="uploadAllImage()" />
    </template>
  </Dialog>

  <Dialog
    header="Thêm ảnh"
    v-model:visible="openAddSlideShowImage"
    :style="{ width: '720px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên</label>
          <InputText spellcheck="false" class="col-10 ip36" v-model="image.Image_Name" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2">Ảnh</label>
          <div class="col-4 p-0">
            <div class="inputanh" @click="chonanh('AnhSlideImage')">
              <img
                id="LogoSlideImage"
                v-bind:src="
                  image.IsFilePath
                    ? basedomainURL + image.IsFilePath
                    : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="AnhSlideImage"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'LogoSlideImage')"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Link</label>
          <InputText
            spellcheck="false"
            :placeholder="image.IsVideo ? 'Nhập link Youtube' : ''"
            class="col-10 ip36"
            v-model="imageis_link"
            @input="changeLinkYoutube()"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mô tả</label>
          <Textarea
            class="col-10 p-2"
            v-model="image.Des"
            rows="3"
            style="vertical-align: middle"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="image.STT" />
          <label class="col-2 text-right">Trạng thái</label>
          <InputSwitch v-model="image.Trangthai" style="vertical-align: text-bottom" />
          <label class="col-2 text-right">Video</label>
          <InputSwitch v-model="image.IsVideo" style="vertical-align: text-bottom" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="openAddSlideShowImage = false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="addSlideShowImage()" />
    </template>
  </Dialog>
  <Dialog
    :header="slideshow.SlideShow_Name+' ('+compVideos.length+')'"
    v-model:visible="showVideo"
    :style="{ width: '720px', zIndex: 1000 }"
    :maximizable="false"
    :autoZIndex="false"
    class="diaglog-black"
    :modal="true"
  >
    <div class="video-slide text-center">
      <div class="video">
        <YouTube style="background-color: rgba(0,0,0,.9);" class="m-auto" v-if="image" :src="imageis_link" />
      </div>
      <div class="thumb-video mt-2">
        <Carousel :value="compVideos" :numVisible="4" :numScroll="4">
          <template #item="item">
            <div class="item text-center p-1">
              <Button class="p-button-text p-button-plain p-0 w-full" @click="clickVideo(item.data)">
                <img
                  :class="item.data.Image_ID==image.Image_ID?'true':''"
                  :src="basedomainURL + item.data.IsFilePath"
                  :alt="item.data.Image_Name"
                  style="display: block; height: 80px;width:100%;object-fit:cover;margin-auto"
                />
              </Button>
              <span style="font-size: 13px">{{ item.data.Image_Name }}</span>
            </div>
          </template>
        </Carousel>
      </div>
    </div>
  </Dialog>
  <Galleria
    v-model:visible="showGalleria"
    :value="compImages"
    :numVisible="9"
    containerStyle="max-width: 60vw;"
    :circular="true"
    :fullScreen="true"
    :showItemNavigators="true"
    :showThumbnails="true"
  >
    <template #item="{ item }">
      <img
        :src="basedomainURL + item.IsFilePath"
        :alt="item.Image_Name"
        style="width: 100%; display: block"
      />
    </template>
    <template #thumbnail="{ item }">
      <div class="grid grid-nogutter justify-content-center">
        <img
          :src="basedomainURL + item.IsFilePathThumb"
          :alt="item.Image_Name"
          style="display: block; height: 60px"
        />
      </div>
    </template>
    <template #caption="{ item }">
      <h4 style="margin-bottom: 0.5rem">{{ item.Image_Name }}</h4>
      <p>{{ item.Des }}</p>
    </template>
  </Galleria>
</template>
<style scoped>
.classslideshow {
  background-color: aliceblue;
}
span.slideshowtrue {
  font-weight: 500;
}
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
}
.chip2 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
}
.ipnone {
  display: none;
}
.inputanh {
  /* border: 1px solid #ccc; */
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.image-slide {
  margin: 0;
}
.image-slide li {
  list-style-type: none;
  padding: 5px;
  display: inline-block;
  position: relative;
}
.image-slide li img {
  height: 100px;
  object-fit: cover;
}
.btremoimage {
  position: absolute;
  top: -10px;
  right: -10px;
  z-index: 1;
  background-color: rgba(0, 0, 0, 0.7);
  border: navajowhite;
}
img.true{border:4px solid orange}
</style>
