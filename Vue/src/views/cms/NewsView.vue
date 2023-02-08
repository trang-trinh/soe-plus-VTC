<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import UploadAdapter from "../../util/UploadAdapter";
import CompViewNew from "./Comp/CompViewNew.vue";
import moment from "moment";
//init Model
const editor = ClassicEditor;
function MyCustomUploadAdapterPlugin(editor) {
  editor.editing.view.change((writer) => {
    writer.setAttribute("spellcheck", "false", editor.editing.view.document.getRoot());
    writer.setStyle("height", "600px", editor.editing.view.document.getRoot());
  });
  editor.plugins.get("FileRepository").createUploadAdapter = (loader) => {
    return new UploadAdapter(
      loader,
      baseURL + "/api/Upload/Update_FileCK",
      `Bearer ${store.getters.token}`
    );
  };
}
const editorConfig = {
  placeholder: "",
  title: {
    placeholder: "",
  },
  toolbar: {
    items: [
      //"fullScreen",
      "heading",
      "removeFormat",
      "|",
      "fontFamily",
      "fontSize",
      "|",
      "alignment",
      "|",
      "fontColor",
      "fontBackgroundColor",
      "|",
      "bold",
      "italic",
      "strikethrough",
      "underline",
      "subscript",
      "superscript",
      "|",
      "link",
      "|",
      "outdent",
      "indent",
      "|",
      "bulletedList",
      "numberedList",
      "todoList",
      "|",
      //"code",
      "codeBlock",
      "|",
      "insertTable",
      "|",
      //"uploadImage",
      "insertImage",
      "mediaEmbed",
      "blockQuote",
      // "MathType",
      // "ChemType",
      "|",
      "specialCharacters",
      "|",
      "undo",
      "redo",
      "sourceEditing",
      "htmlEmbed",
    ],
  },
  //extraPlugins: [FullScreen],
  removePlugins: ["MediaEmbedToolbar", "Title", "Markdown"],
  resize_enabled: true,
};
const tdNewTrangthais = ref([
  { value: 0, text: "Chưa duyệt" },
  { value: 1, text: "Duyệt" },
  { value: -1, text: "Không duyệt" },
]);
let objNewTrangthais = { 0: "Chưa duyệt", 1: "Đã duyệt", "-1": "Không duyệt" };
const tdNewTypes = ref([
  { value: 0, text: "Tin thường" },
  { value: 1, text: "Tin ảnh" },
  { value: 2, text: "Tin video" },
]);
const tintuc = ref({
  News_Name: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  News_Name: {
    required,
  },
};
const v$ = useVuelidate(rules, tintuc);
//Khai báo biến
const store = inject("store");
const selectTopic = ref();
const selectedNodes = ref([]);
const treetopics = ref([]);
const filters = ref({});
const opition = ref({
  Title: "Tin tức",
  search: "",
  rSearch: "",
  rPageNo: 0,
  PageNo: 1,
  PageSize: 20,
  rPageSize: 50,
  Lang_ID: store.getters.lang.Lang_ID,
  Donvi_ID: store.getters.user.Donvi_ID,
  user_id: store.getters.user.user_id,
});
const tintucs = ref();
const displayAddNews = ref(false);
const isFirst = ref(true);
let files = {};
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
      exportNews("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportNews("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.News_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.News_ID), 1);
};
const handleFileUpload = (event, ia) => {
  files[ia] = event.target.files[0];
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
//Show Modal
const modeldate = ref({});
const showModalAddNews = () => {
  modeldate.value = {};
  submitted.value = false;
  selectedTintucs.value = [];
  tintuc.value = {
    News_Name: "",
    STT: tintucs.value.length + 1,
    Trangthai: store.getters.user.IsAdminTruong ? 1 : 0,
    Lang_ID: store.getters.lang.Lang_ID,
    Donvi_ID: store.getters.user.Donvi_ID,
    NewType: 0,
    IsHot: false,
  };
  displayAddNews.value = true;
  setTimeout(() => {
    tintuc.value.showCK = true;
  }, 100);
};
const closedisplayAddNews = () => {
  displayAddNews.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadNews(true);
};
const onSearch = () => {
  loadNews(true);
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadNews(true);
};
const selectedTintucs = ref();
const rltintucs = ref([]);
const onLazyLoad = (event) => {
  if (
    opition.value.rtotalRecords > 0 &&
    opition.value.rtotalRecords <= rltintucs.value.length
  ) {
    opition.value.rlloading = false;
  } else {
    opition.value.rPageNo += 1;
    loadRealteNews();
  }
};
const loadRealteNews = () => {
  opition.value.rlloading = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_News_List",
        par: [
          { par: "PageNo", va: opition.value.rPageNo },
          { par: "PageSize", va: opition.value.rPageSize },
          { par: "Search", va: opition.value.rSearch },
          { par: "Topic_ID", va: -1 },
          { par: "Lang_ID", va: opition.value.Lang_ID },
          { par: "Donvi_ID", va: opition.value.Donvi_ID },
          { par: "Trangthai", va: 1 },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        opition.value.rlloading = false;
        rltintucs.value = rltintucs.value.concat(data[0]);
        opition.value.rtotalRecords = data[1][0].totalRecords;
      } else {
        rltintucs.value = [];
      }
      opition.value.rlloading = false;
    })
    .catch((error) => {});
};
const funselectTreeTopic=(event)=>{
  opition.value.Topic_ID=event.key;
  opition.value.Title=event.label;
}
const loadNews = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_News_List",
        par: [
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Search", va: opition.value.search },
          { par: "Topic_ID", va: opition.value.Topic_ID },
          { par: "Lang_ID", va: opition.value.Lang_ID },
          { par: "Donvi_ID", va: opition.value.Donvi_ID },
          { par: "Trangthai", va: opition.value.Trangthai },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((element, i) => {
          element.STT = (opition.value.PageNo - 1) * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.CreateDate)).format(
            "DD/MM/YYYY HH:mm:ss"
          );
        });
        opition.value.loading = false;
        tintucs.value = data[0];
        opition.value.totalRecords = data[1][0].totalRecords;
      } else {
        tintucs.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const displayViewNew = ref(false);
const chonTintucs = ref([]);
const viewNews = (md) => {
  tintuc.value = md;
  displayViewNew.value = true;
};
const editNews = (md) => {
  tintuc.value = md;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddNews.value = true;

  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "CMS_News_Get", par: [{ par: "News_ID", va: md.News_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Keywords) obj.Keywords = obj.Keywords.split(",");
        modeldate.value.ngaydang = [];
        if (obj.StartDate) {
          modeldate.value.ngaydang.push(new Date(obj.StartDate));
        }
        if (obj.EndDate) {
          modeldate.value.ngaydang.push(new Date(obj.EndDate));
        }
        if (modeldate.value.ngaydang.length == 0) {
          modeldate.value.ngaydang = null;
        }
        chonTintucs.value = data[1];
        selectedTintucs.value = [];
        if (obj.Relate_ID) {
          let arrI = [];
          obj.Relate_ID.split(",").forEach((op) => {
            arrI.push(parseInt(op));
          });
          selectedTintucs.value = arrI;
          opition.value.rPageNo = 1;
          loadRealteNews();
        }
        tintuc.value = obj;
        selectTopic.value = {};
        selectTopic.value[
          tintuc.value.Topic_ID != null ? tintuc.value.Topic_ID : "-1"
        ] = true;
        setTimeout(() => {
          tintuc.value.showCK = true;
        }, 500);
      }
    })
    .catch((error) => {
      console.log(error);
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
    tintuc.value.Topic_ID = keys[0];
    if (tintuc.value.Topic_ID == -1) {
      tintuc.value.Topic_ID = null;
    }
  }
  if (selectedTintucs.value) {
    tintuc.value.Relate_ID = selectedTintucs.value.join(",");
  }
  addNews();
};

const upTrangthaiNews = (md, tt) => {
  let ids = [];
  let tts = [];
  if (md == null) {
    selectedNodes.value.forEach((n) => {
      ids.push(n.News_ID);
      tts.push(tt);
    });
  } else {
    ids = [md.News_ID];
    tts = [tt];
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/News/Update_TrangthaiNews",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái biên mục thành công!");
        loadNews(true);
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

const upTrangthaiNewsHot = (md, tt) => {
  let ids = [];
  let tts = [];
  if (md == null) {
    selectedNodes.value.forEach((n) => {
      ids.push(n.News_ID);
      tts.push(tt);
    });
  } else {
    ids = [md.News_ID];
    tts = [tt];
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/News/Update_TrangthaiNewsHot",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng tin nổi bật thành công!");
        loadNews(true);
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

const addNews = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  for (var k in files) {
    let file = files[k];
    formData.append(k, file);
  }
  let md = { ...tintuc.value };
  if (modeldate.value.ngaydang) {
    if (modeldate.value.ngaydang.length > 0) {
      md.StartDate = modeldate.value.ngaydang[0];
    }
    if (modeldate.value.ngaydang.length > 1) {
      md.EndDate = modeldate.value.ngaydang[1];
    }
  }
  if (md.Keywords instanceof Array) {
    md.Keywords = md.Keywords.join(",");
  }
  formData.append("model", JSON.stringify(md));
  axios({
    method: tintuc.value.News_ID ? "put" : "post",
    url: baseURL + `/api/News/${tintuc.value.News_ID ? "Update_News" : "Add_News"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật tin thành công!");
        loadNews();
        closedisplayAddNews();
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

const delNews = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tin này không!",
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
          .delete(baseURL + "/api/News/Del_News", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.News_ID] : selectedNodes.value.map((x) => x.News_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá tin thành công!");
              loadNews();
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

const exportNews = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "CMS_News" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH BIÊN MỤC",
        proc: "CMS_News_ListExport",
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
const rowClass = (data) => {
  return data.Trangthai == -1 ? "error" : data.Cap == 1 ? "" : "";
};
const renderTree = (data, paid, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x[paid] == null || data.findIndex((a) => a[id] == x[paid]) == -1)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x[paid] == pid);
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
        let dts = data.filter((x) => x[paid] == pid);
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
const loadTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_News_ListTudien",
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
        let obj = renderTree(data, "Parent_ID", "Topic_ID", "Topic_Name", "Biên mục");
        treetopics.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {});
};
//Emit lang
const emitter = inject("emitter");
emitter.on("lang", (obj) => {
  loadNews(true);
});
const onReady = (editor) => {
  editor.editing.view.change((writer) => {
    writer.setStyle("height", "450px", editor.editing.view.document.getRoot());
    writer.setAttribute("spellcheck", "false", editor.editing.view.document.getRoot());
  });
};
//Duyệt
let nodeSelect;
const menuButDuyet = ref();
const itemButDuyet = ref([
  {
    label: "Duyệt",
    icon: "pi pi-check-square",
    command: () => {
      setDuyet(1);
    },
  },
  {
    label: "Không duyệt",
    icon: "pi pi-stop",
    command: () => {
      setDuyet(-1);
    },
  },
  {
    label: "Chờ duyệt",
    icon: "pi pi-refresh",
    command: () => {
      setDuyet(0);
    },
  },
  {
    label: "Tin nổi bật",
    icon: "pi pi-star-fill",
    command: () => {
      setHot(true);
    },
  },
  {
    label: "Tin thường",
    icon: "pi pi-star",
    command: () => {
      setHot(false);
    },
  },
]);
const toggleDuyet = (event, data) => {
  if (store.getters.user.IsAdminTruong) {
    nodeSelect = { ...data };
    menuButDuyet.value.toggle(event);
  }
};
const setHot = (tt) => {
  if (nodeSelect.News_ID) {
    nodeSelect.IsHot = tt;
    upTrangthaiNewsHot(nodeSelect,tt);
  } else {
    upTrangthaiNewsHot(null, tt);
  }
};
const setDuyet = (tt) => {
  if (nodeSelect.News_ID) {
    nodeSelect.Trangthai = tt;
    upTrangthaiNews(nodeSelect,tt);
  } else {
    upTrangthaiNews(null, tt);
  }
};
onMounted(() => {
  //init
  loadNews(true);
  loadTudien();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="tintucs"
      :paginator="opition.totalRecords > opition.PageSize"
      :loading="opition.loading"
      :totalRecords="opition.totalRecords"
      :rows="opition.PageSize"
      dataKey="News_ID"
      :showGridlines="true"
      :rowHover="true"
      v-model:selection="selectedNodes"
      :filters="filters"
      filterMode="lenient"
      :rowsPerPageOptions="[10, 20, 40]"
      :lazy="true"
      :row-class="rowClass"
      @page="onPage($event)"
      :pageLinkSize="opition.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="tintuc-title mt-0 ml-1 mb-2">
          <i class="pi pi-video"></i> {{ opition.Title }} ({{ opition.totalRecords }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <TreeSelect
              class="mr-2"
              v-model="selectTopic"
              :options="treetopics"
              :filter="true"
              :showClear="true"
              placeholder="Chọn biên mục"
              @change="loadNews(true)"
              @node-select="funselectTreeTopic"
              optionLabel="Topic_Name"
              optionValue="Topic_ID"
            ></TreeSelect>
            <Dropdown
              class="mr-2"
              v-model="opition.Trangthai"
              :options="tdNewTrangthais"
              optionLabel="text"
              optionValue="value"
              :showClear="true"
              @change="loadNews(true)"
              placeholder="Chọn trạng thái"
            />
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
              v-tooltip="'Cập nhật trạng thái'"
              icon="pi pi-check-circle"
              class="mr-2"
              v-if="store.getters.user.IsAdminTruong && selectedNodes.length > 0"
              @click="toggleDuyet($event)"
            />
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />
            <Button
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
              v-if="selectedNodes.length > 0"
              @click="delNews()"
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
              label="Thêm tin"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddNews"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px"
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="IsHot"
        header=""
        class="align-items-center justify-content-center text-center"
        :sortable="true"
        headerStyle="text-align:center;max-width:40px"
        bodyStyle="text-align:center;max-width:40px"
      >
        <template #body="md">
          <Button
            style="padding: 0 !important"
            @click="upTrangthaiNewsHot(md.data, md.data.IsHot ? false : true)"
            class="p-button-text p-button-secondary"
          >
            <i v-if="md.data.IsHot" class="pi pi-star-fill" style="color: orange"></i>
            <i v-else class="pi pi-star"></i>
          </Button>
        </template>
      </Column>
      <Column
        field="News_ID"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
      </Column>
      <Column
        field="Image"
        header="Ảnh"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      >
        <template #body="md">
          <img :src="basedomainURL + md.data.Image" width="70" />
        </template>
      </Column>
      <Column
        field="News_Name"
        header="Tên tin"
        :sortable="true"
        class="align-items-start"
      >
      </Column>
      <Column
        :sortable="true"
        field="Ngay"
        header="Ngày tạo"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:170px"
        bodyStyle="text-align:center;max-width:170px"
      >
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="md">
          <Button
            @click="toggleDuyet($event, md.data)"
            class="p-button-text p-button-secondary"
          >
            <Chip
              :label="objNewTrangthais[md.data.Trangthai]"
              :class="'chip' + md.data.Trangthai"
            />
          </Button>
        </template>
      </Column>
      <Column
        headerClass="text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-eye"
            class="p-button-rounded p-button-sm p-button-secondary"
            style="margin-right: 0.5rem"
            @click="viewNews(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editNews(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delNews(md.data)"
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
  <Menu id="overlay_Duyet" ref="menuButDuyet" :model="itemButDuyet" :popup="true" />

  <Dialog
    header="Cập nhật tin"
    v-model:visible="displayAddNews"
    :style="{ width: '1024px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tiêu đề <span class="redsao">(*)</span></label>
          <Textarea
            :class="{ 'p-invalid': v$.News_Name.$invalid && submitted }"
            spellcheck="false"
            style="vertical-align: middle"
            class="col-10 p-1"
            v-model="tintuc.News_Name"
            rows="2"
          />
        </div>
        <small
          v-if="(v$.News_Name.$invalid && submitted) || v$.News_Name.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.News_Name.required.$message
                .replace("Value", "Tên tin")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="col-8">
          <div class="field">
            <label class="col-3 text-left">Biên mục</label>
            <TreeSelect
              class="col-8"
              v-model="selectTopic"
              :options="treetopics"
              :showClear="true"
              :filter="true"
              placeholder=""
              optionLabel="data.Topic_Name"
              optionValue="data.Topic_ID"
            ></TreeSelect>
          </div>
          <div class="field">
            <label class="col-3 text-left">Loại</label>
            <Dropdown
              class="col-8"
              v-model="tintuc.NewType"
              :options="tdNewTypes"
              optionLabel="text"
              optionValue="value"
            />
          </div>
          <div class="field">
            <label class="col-3 text-left">Ngày đăng </label>
            <Calendar
              class="col-8 ml-0 p-0"
              id="ngaydang"
              v-model="modeldate.ngaydang"
              selectionMode="range"
              :showIcon="true"
              :showTime="true"
              :manualInput="true"
            />
          </div>
        </div>
        <div class="col-4">
          <div class="field">
            <label class="col-12 text-rigth">Ảnh đại diện</label>
            <div class="inputanh" @click="chonanh('AnhTin')">
              <img
                id="tinAnh"
                v-bind:src="
                  tintuc.Image
                    ? basedomainURL + tintuc.Image
                    : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="AnhTin"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'tinAnh')"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mô tả</label>
          <Textarea
            spellcheck="false"
            style="vertical-align: middle"
            class="col-10 p-1"
            v-model="tintuc.Contents"
            rows="3"
          />
        </div>
        <div class="field flex col-12 md:col-12">
          <label class="col-2 text-left">Nội dung</label>
          <div class="col-10 p-0" style="min-height: 300px">
            <ckeditor
              v-if="tintuc.showCK"
              spellcheck="false"
              :editor="editor"
              v-model="tintuc.Details"
              :config="editorConfig"
              @ready="onReady"
            ></ckeditor>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Từ khoá</label>
          <Chips
            spellcheck="false"
            class="col-10 p-0"
            v-model="tintuc.Keywords"
            :addOnBlur="true"
            separator=","
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Nguồn</label>
          <InputText spellcheck="false" class="col-10 ip36" v-model="tintuc.IsWriter" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="tintuc.STT" />
          <label v-if="store.getters.user.IsAdminTruong" class="col-2 text-right"
            >Trạng thái</label
          >
          <Dropdown
            v-if="store.getters.user.IsAdminTruong"
            class="col-3"
            v-model="tintuc.Trangthai"
            :options="tdNewTrangthais"
            optionLabel="text"
            optionValue="value"
          />
          <label class="col-2 text-right">Nổi bật</label>
          <InputSwitch
            class="col-2"
            style="vertical-align: middle"
            v-model="tintuc.IsHot"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tin liên quan</label>
          <MultiSelect
            v-model="selectedTintucs"
            :options="rltintucs"
            :filter="true"
            :showClear="true"
            selectedItemsLabel="{0} tin đã chọn"
            :maxSelectedLabels="3"
            optionLabel="News_Name"
            optionValue="News_ID"
            dataKey="News_ID"
            display="chip"
            class="col-10"
            :virtualScrollerOptions="{
              lazy: true,
              onLazyLoad: onLazyLoad,
              itemSize: 38,
              loading: opition.rlloading,
              delay: 250,
            }"
            placeholder="Chọn tin liên quan"
          >
          </MultiSelect>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddNews"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog
    v-model:visible="displayViewNew"
    :style="{ width: '1024px', zIndex: 1 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <template #header
      ><h2 class="new-title m-0 p-0">{{ tintuc.News_Name }}</h2>
    </template>
    <CompViewNew :proptintuc="tintuc" :showTitle="false"></CompViewNew>
  </Dialog>
</template>
<style scoped>
.new-title {
  font-size: 15pt;
  font-weight: bold;
  color: #000;
}
span.tintuctrue {
  font-weight: 500;
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
.chip-1 {
  background-color: red;
  color: #fff;
  font-size: 0.875rem;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
}
.chip0 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
}
</style>
