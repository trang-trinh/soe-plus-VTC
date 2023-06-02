<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  plugin_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  plugin_name: {
    required,
    $errors: [
      {
        $property: "plugin_name",
        $validator: "required",
        $message: "Tên thư viện không được để trống!",
      },
    ],
  },
};
const plugin = ref({
  plugin_name: "",
  images: "",
  status: true,
  is_app: false,
  is_order: 1,
  des: "",
  is_url: "",
  keywords: null,
});
const category = ref({
  category_name: "",
  is_order: 1,
  status: true,
});

const rulesCate = {
  category_name: {
    required,
    $errors: [
      {
        $property: "category_name",
        $validator: "required",
        $message: "Tên loại không được để trống!",
      },
    ],
  },
};
const isSaveCategory = ref(false);
const listImg = ref([]);
const showThumbnails = ref(false);
const selectedPlugins = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, plugin);
const validatePlugin = useVuelidate(rulesCate, category);
const isSavePlugin = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const listThumbnails = ref([]);
const options = ref({
  IsNext: true,
  sort: "plugin_id",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});

//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_plugin_count",
        par: [{ par: "search", va: options.value.SearchText }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;

        sttPlugin.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "Plugin.vue",
        log_content: error.message,
      });
    });
};
//Lấy dữ liệu thư viện
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNo) {
    //Trang sau

    options.value.id = datalists.value[datalists.value.length - 1].plugin_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].plugin_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadProject();
};
//Hiển thị dialog
const listPlugin = ref([]);
const headerDialog = ref();
const displayBasic = ref(false);
const addPlugin = (str) => {
  files = [];
  let sttSer = 0;
  submitted.value = false;
  checkImage.value = true;
  showThumbnails.value = false;
  if (nodeSelected.value) {
    let stt = 0;
    listPlugin.value.forEach((element) => {
      if (element.category_id == nodeSelected.value.category_id) {
        stt++;
      }
    });
    sttSer = stt + 1;
  }
  plugin.value = {
    category_id: nodeSelected.value.category_id,
    plugin_name: "",
    images: "",
    status: true,
    is_default: false,
    is_order: sttSer,
    des: "",
    is_url: "",
    keywords: null,
  };
  listThumbnails.value = [
    {
      id: 0,
      itemImageSrc: "/src/assets/image/noimg.jpg",
      thumbnailImageSrc: "/src/assets/image/noimg.jpg",
      alt: "Description for Image 1",
      title: "Title 1",
    },
  ];

  checkIsmain.value = false;
  isSavePlugin.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};

const editCategory = (value) => {
  submitted.value = false;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_category_get",
        par: [{ par: "category_id", va: value }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      isChirlden.value = false;
      category.value = data[0];
      if (category.value.parent_id != null) {
        listCategorySave.value.forEach((element) => {
          if (element.category_id == category.value.parent_id) {
            nameParent.value = element.category_name;
            return;
          }
        });
        isChirlden.value = true;
      }
      headerCate.value = "Sửa loại thư viện";
      isSaveCategory.value = true;
      displayCate.value = true;
    });
};
const deleteCategory = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá loại thư viện này không!",
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
          .delete(baseURL + "/api/api_category/Delete_category", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value != null ? [value] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá loại thư viện thành công!");
              nodeSelected.value = null;

              loadProject();
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const closeCategory = () => {
  category.value = ref({
    category_name: "",
    is_order: 1,
    status: true,
  });
  displayCate.value = false;
};
const closeDialog = () => {
  plugin.value = {
    plugin_name: "",
    images: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
};
//Lấy file logo
const chonanh = (id) => {
  document.getElementById(id).click();
};
const checkImage = ref(false);
const handleFileUpload = (event) => {
  for (let index = 0; index < event.target.files.length; index++) {
    files.push(event.target.files[index]);
  }
  if (files.length == 0) {
    return;
  }

  if (listThumbnails.value.length == 1 && listThumbnails.value[0].id == 0) {
    listThumbnails.value = [];
  }
  if (checkImage.value == true) {
    listThumbnails.value = [];
  }
  let id = listThumbnails.value.length;
  var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  if (files.length > 0) {
    for (let index = 0; index < files.length; index++) {
      if (allowedExtensions.exec(files[index].name)) {
        let image = ref();
        id += 1;
        listImg.value.push(files[index]);
        image.id = id;
        image.itemImageSrc = URL.createObjectURL(files[index]);
        image.thumbnailImageSrc = URL.createObjectURL(files[index]);
        image.alt = files[index].name;
        image.title = files[index].name + index;
        listThumbnails.value.push(image);
      }
    }
  }

  showThumbnails.value = true;
};
//Thêm bản ghi
let files = [];
const sttPlugin = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url", file);
  }
  if (plugin.value.keywords != null) {
    plugin.value.keywords = plugin.value.keywords.toString();
  }
  if (Array.isArray(plugin.value.file_url) && plugin.value.file_url != null) {
    plugin.value.file_url = plugin.value.file_url.toString();
  }
  if (Array.isArray(plugin.value.images) && plugin.value.images != null) {
    plugin.value.images = plugin.value.images.toString();
  }

  formData.append("plugin", JSON.stringify(plugin.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSavePlugin.value) {
    axios
      .post(baseURL + "/api/api_plugin/Add_plugin", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thư viện thành công!");
          loadProject();
         
          closeDialog();
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
  } else {
    axios
      .put(baseURL + "/api/api_plugin/Update_plugin", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thư viện thành công!");
         
          loadProject();

          closeDialog();
        } else {
          console.log("LỖI A:", response);
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
  }
};
const projectSelected = ref();
const listCategorySave = ref([]);
const listProject = ref([]);
const checkIsmain = ref(true);
//Sửa bản ghi
const editPlugin = (dataTem) => {
  files = [];
  checkImage.value = false;
  submitted.value = false;
  showThumbnails.value = true;
  if (dataTem.keywords != null && dataTem.keywords.length > 1) {
    if (!Array.isArray(dataTem.keywords)) {
      dataTem.keywords = dataTem.keywords.split(",");
    }
  }

  let arrImg = [];
  if (dataTem.images != null && dataTem.images.length > 1) {
    if (!Array.isArray(dataTem.images)) {
      let arr = dataTem.images.split(",");
      dataTem.images = arr;
      arr.forEach((element, i) => {
        arrImg.push({
          id: i + 1,
          itemImageSrc: baseURL + element,
          thumbnailImageSrc: baseURL + element,
          alt: element,
          title: baseURL + element,
        });
      });

      listThumbnails.value = arrImg;
    } else {
      dataTem.images.forEach((element, i) => {
        arrImg.push({
          id: i + 1,
          itemImageSrc: baseURL + element,
          thumbnailImageSrc: baseURL + element,
          alt: element,
          title: baseURL + element,
        });
      });
      listThumbnails.value = arrImg;
    }
  }
  if (!Array.isArray(plugin.value.file_url) && plugin.value.file_url != null) {
    dataTem.file_url = dataTem.file_url.split(",");
  }
  plugin.value = dataTem;
  headerDialog.value = "Sửa thư viện";
  isSavePlugin.value = true;
  displayBasic.value = true;
};
//Xuất excel
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ExportExcel");
    },
  },
  {
    label: "Import Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ImportExcel");
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH TEM",
        proc: "api_plugin_listexport",
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  let data = {
    id: options.value.id,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Plugin", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            (options.value.PageNo - 1) * options.value.PageSize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchPlugin = (event) => {
  if (event.code == "Enter") {
    options.value.loading = true;
    loadProject();
  }
};
const refreshPlugin = () => {
  options.value.loading = true;
  loadProject();
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "plugin_name" ? "plugin_name" : key,
        filteroperator: value.operator,
        filterconstraints: value.constraints,
      };

      if (value.value && value.value.length > 0) {
        obj.filteroperator = value.matchMode;
        obj.filterconstraints = [];
        value.value.forEach(function (vl) {
          obj.filterconstraints.push({ value: vl[obj.key] });
        });
      } else if (value.matchMode) {
        obj.filteroperator = "and";
        obj.filterconstraints = [value];
      }
      if (
        obj.filterconstraints &&
        obj.filterconstraints.filter((x) => x.value != null).length > 0
      )
        filterSQL.value.push(obj);
    }
  }
  options.value.PageNo = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedPlugins.length);
  let checkD = false;
  selectedPlugins.value.forEach((item) => {
    if (item.is_default) {
      toast.error("Không được xóa thư viện mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thư viện này không!",
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

          selectedPlugins.value.forEach((item) => {
            listId.push(item.plugin_id);
          });
          axios
            .delete(baseURL + "/api/api_plugin/Delete_plugin", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thư viện thành công!");
                checkDelList.value = false;

                loadProject();
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
                  title: "Error!",
                  text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
};

//Filter
const showFilter = ref(false);
const toggleFilter = (event) => {
  if (showFilter.value) {
    showFilter.value = false;
  } else {
    showFilter.value = true;
  }
};
const filterButs = ref();
const itemfilterButs = ref([
  {
    label: "Phân loại",
    check: true,
  },
  {
    label: "Trạng thái",
    check: false,
  },
]);

const trangThai = ref([
  { name: "Có", code: 1 },
  { name: "Không", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);

watch(selectedPlugins, () => {
  if (selectedPlugins.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
watch(listThumbnails, () => {
  if (listThumbnails.value.length < 1) {
    listThumbnails.value = [
      {
        id: 0,
        itemImageSrc: "/src/assets/image/noimg.jpg",
        thumbnailImageSrc: "/src/assets/image/noimg.jpg",
        alt: "Description for Image 1",
        title: "Title 1",
      },
    ];
  }
});

const layout = ref("list");
const checkEditPlugin = ref();
const toggleMores = (event, u, check) => {
  if (check) {
    category.value = u;
    checkCateEdit.value = true;
  } else {
    plugin.value = u;
    checkCateEdit.value = false;
  }
  menuButMores.value.toggle(event);
};
const checkCateEdit = ref();
const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Sửa",
    icon: "pi pi-cog",
    command: (event) => {
      if (checkCateEdit.value) {
        editCategory(category.value.category_id);
      } else {
        console.log("Dữ lể", event);
        editPlugin(plugin.value);
      }
    },
  },

  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      if (checkEditPlugin.value) {
        deleteCategory(plugin.value.plugin_id);
      } else {
        deletePlugin(plugin.value.plugin_id);
      }
    },
  },
]);

const reloadPlugin = () => {
  options.value.loading = true;
  datalists.value = [];
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_category_list",
          par: [
            { par: "parent_id", va: nodeSelected.value.category_id },
            { par: "project_id", va: projectSelected.value },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          datalists.value.push(element);
        });
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });

    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_plugin_list",
          par: [
            { par: "category_id", va: nodeSelected.value.category_id },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (data.length > 0) {
          data.forEach((element) => {
            if (element.keywords != null && element.keywords.length > 1) {
              if (!Array.isArray(element.keywords)) {
                element.keywords = element.keywords.split(",");
              }
            }
            if (element.images != null && element.images.length > 1) {
              if (!Array.isArray(element.images)) {
                element.images = element.images.split(",");
              }
            }
            datalists.value.push(element);
          });
        }

        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
        console.log("Ở",datalists.value);
    options.value.totalRecords = datalists.value.length;
  })();

};
const deletePlugin = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thư viện này không!",
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
        console.log("RAW2", plugin.value);
        axios
          .delete(baseURL + "/api/api_plugin/Delete_plugin", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value != null ? [value] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thư viện thành công!");
              reloadPlugin();
              loadProject();
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const onUploadFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = files.filter((a) => a != event.file);
};
const listCategory = ref([]);
const database_name = ref();
const projectLogo = ref();

const renderPlugin = (listCate, listPlug) => {
  listPlug.forEach((element) => {
    if (element.keywords != null && element.keywords.length > 1) {
      if (!Array.isArray(element.keywords)) {
        element.keywords = element.keywords.split(",");
      }
    }
    if (element.images != null && element.images.length > 1) {
      if (!Array.isArray(element.images)) {
        element.images = element.images.split(",");
      }
    }
  });

  let arrChils = [];
  listCate
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = { key: m.category_id, data: m };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        let dts = listCate.filter((x) => x.parent_id == category_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = { key: em.category_id, data: em };
            rechildren(om1, em.category_id);
            mm.children.push(om1);
          });
        }
        if (listPlug.length > 0) {
          let dsv = listPlug.filter((x) => x.category_id == category_id);
          if (dsv.length > 0) {
            dsv.forEach((em) => {
              let om1 = { key: em.plugin_name, data: em };
              mm.children.push(om1);
            });
          }
        }
      };
      rechildren(om, m.category_id);
      arrChils.push(om);
    });

  //   arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn Module----" });
  listCategory.value = arrChils;
};
const loadProject = () => {
  (async () => {
    listProject.value = [];
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_project_list_api",
          par: [
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        projectSelected.value = data[0].project_id;
        database_name.value = data[0].db_name;
        projectLogo.value = data[0].project_logo;
        data.forEach((element) => {
          let db = {
            name: element.project_name,
            code: element.project_id,
            db_name: element.db_name,
            project_logo: element.project_logo,
          };
          listProject.value.push(db);
        });
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });

    listCategory.value = [];
    listCategorySave.value = [];
    let listCate = [];

    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_category_list",
          par: [
            { par: "parent_id", va: options.value.parent_id },
            { par: "project_id", va: projectSelected.value },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listCate = data;
        listCategorySave.value = data;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_plugin_list_all",
          par: [
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (isFirst.value) isFirst.value = false;
        listPlugin.value = data;
        renderPlugin(listCate, data);
        options.value.loading = false;
      })
      .catch((error) => {
        console.log(error);
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const isChirlden = ref(false);
const categoryIdSave = ref();
const selectedKey = ref();
const expandedKeys = ref({});
const nodeValue = ref();
const categoryName = ref();
const checkNode = ref(false);
const keyselected = ref();
const onNodeSelect = (node, check) => {
  keyselected.value = node.key;
  if (selectedKey.value) {
    selectedKey.value[keyselected.value] = false;
    selectedKey.value[node.key] = true;
  }
  if (check) {
    if (expandedKeys.value[node.key] == true) {
      expandedKeys.value[node.key] = false;
    } else {
      expandedKeys.value[node.key] = true;
    }
  }
  checkNode.value = true;
  nodeValue.value = node;
  options.value.loading = true;
  categoryName.value = node.data.category_name;
  if (node.data.plugin_id) {
    listImgPlugin.value = [];
    if (!Array.isArray(node.data.images))
      node.data.images = node.data.images.split(",");
    for (let index = 0; index < node.data.images.length; index++) {
      const element = node.data.images[index];
      let image = {};
      image.id = index + 1;
      image.itemImageSrc = baseURL + element;
      image.thumbnailImageSrc = baseURL + element;
      image.alt = element;
      image.title = element;
      listImgPlugin.value.push(image);
    }
    if (!Array.isArray(node.data.file_url))
      node.data.file_url = node.data.file_url.split(",");
    node.key = node.data.plugin_name;
    selectedKey.value[node.key] = true;
    keyselected.value = node.key;
    datalists.value.forEach(function (d) {
      d.active = false;

      if (d.plugin_id == node.data.plugin_id) {
        d.active = true;
      }
    });
    node.data.active = true;
    plugin.value = node.data;
    isCheckPlugin.value = true;
  } else {
    if (node.data.category_id == categoryIdSave.value) {
      return;
    } else {
      isTypeAPI.value = true;
      nodeSelected.value = node.data;
      datalists.value = [];
      categoryIdSave.value = node.data.category_id;
      (async () => {
        await axios
          .post(
            baseURL + "/api/Proc/CallProc",
            {
              proc: "api_category_list",
              par: [
                { par: "parent_id", va: node.data.category_id },
                { par: "project_id", va: projectSelected.value },
                { par: "search", va: options.value.SearchText },
                { par: "status", va: options.value.Status },
              ],
            },
            config
          )
          .then((response) => {
            let data = JSON.parse(response.data.data)[0];
             let arr = [];
            data.forEach((element) => {
              listPlugin.value.forEach((item) => {
                if (element.category_id == item.category_id) arr.push(item);
              });
            });
            listPlugin.value.forEach((item) => {
              if (item.category_id == node.data.category_id) arr.push(item);
            });
            arr.forEach((element) => {
              if (element.keywords != null && element.keywords.length > 1) {
                if (!Array.isArray(element.keywords)) {
                  element.keywords = element.keywords.split(",");
                }
              }
                      if (element.images != null && element.images.length > 1) {
                  if (!Array.isArray(element.images)) {
                    element.images = element.images.split(",");
                  }
                }
              datalists.value.push(element);
            });
               options.value.loading = false;
            // data.forEach((element) => {
            //   datalists.value.push(element);
            // });
            // listPlugin
          })
          .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;

            if (error && error.status === 401) {
              swal.fire({
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
              store.commit("gologout");
            }
          });

        // await axios
        //   .post(
        //     baseURL + "/api/Proc/CallProc",
        //     {
        //       proc: "api_plugin_list",
        //       par: [
        //         { par: "category_id", va: node.data.category_id },
        //         { par: "search", va: options.value.SearchText },
        //         { par: "status", va: options.value.Status },
        //       ],
        //     },
        //     config
        //   )
        //   .then((response) => {
        //     let data = JSON.parse(response.data.data)[0];

        //     if (data.length > 0) {
        //       data.forEach((element) => {
        //         if (element.keywords != null && element.keywords.length > 1) {
        //           if (!Array.isArray(element.keywords)) {
        //             element.keywords = element.keywords.split(",");
        //           }
        //         }
        //         if (element.images != null && element.images.length > 1) {
        //           if (!Array.isArray(element.images)) {
        //             element.images = element.images.split(",");
        //           }
        //         }
        //         datalists.value.push(element);
        //       });
        //     }

        //     options.value.loading = false;
        //   })
        //   .catch((error) => {
        //     toast.error("Tải dữ liệu không thành công!");
        //     options.value.loading = false;
        //     console.log(error);
        //     if (error && error.status === 401) {
        //       swal.fire({
        //         title: "Error!",
        //         text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
        //         icon: "error",
        //         confirmButtonText: "OK",
        //       });
        //       store.commit("gologout");
        //     }
        //   });
        options.value.totalRecords = datalists.value.length;
      })();
    }
  }
};
const nameParent = ref();
const onUnNodeSelect = (node) => {};
const isTypeAPI = ref(true);

const loadCategory = () => {
  let listCate = [];
  (async () => {
    listCategorySave.value = [];
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_category_list",
          par: [
            { par: "parent_id", va: options.value.parent_id },
            { par: "project_id", va: projectSelected.value },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listCategorySave.value = data;
        listCate = data;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_plugin_list_all",
          par: [
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (isFirst.value) isFirst.value = false;
        data.forEach((element) => {
          if (element.keywords != null && element.keywords.length > 1) {
            if (!Array.isArray(element.keywords)) {
              element.keywords = element.keywords.split(",");
            }
          }
        });

        renderPlugin(listCate, data);
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const refreshTypeApi = () => {
  options.value.loading = true;
  loadProject();
  onNodeSelect(nodeValue.value, false);
};
const nodeSelected = ref();
const headerCate = ref();
const displayCate = ref();
const addCategory = (str) => {
  submitted.value = false;
  headerCate.value = str;
  let sttCate = listCategory.value.length + 1;
  if (nodeSelected.value) {
    let stt = 0;
    listCategorySave.value.forEach((element) => {
      if (element.parent_id == nodeSelected.value.category_id) {
        stt++;
      }
    });
    sttCate = stt + 1;
  }
  category.value = {
    category_name: "",
    is_order: sttCate,
    status: true,
    parent_id:
      nodeSelected.value != null ? nodeSelected.value.category_id : null,
    project_id: projectSelected.value,
  };
  isChirlden.value = false;
  if (category.value.parent_id != null) {
    listCategorySave.value.forEach((element) => {
      if (element.category_id == category.value.parent_id) {
        nameParent.value = element.category_name;
        isChirlden.value = true;
        return;
      }
    });
  }
  isSaveCategory.value = false;
  displayCate.value = true;
};

const saveCategory = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveCategory.value) {
    axios
      .post(baseURL + "/api/api_category/Add_category", category.value, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm loại API thành công!");
          loadProject();
          reloadPlugin();
          closeCategory();
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
  } else {
    axios
      .put(
        baseURL + "/api/api_category/Update_category",
        category.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa loại API thành công!");

          loadProject();
          closeCategory();
        } else {
          console.log("LỖI A:", response);
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
  }
};
const refreshService = () => {
  if (nodeValue.value) {
    options.value.loading = true;
    onNodeSelect(nodeValue.value, false);
  }
};
const deleteFileCode = (value) => {
  plugin.value.file_url = plugin.value.file_url.filter((a) => a != value);
};
const activeIndex = ref(0);
const list = ref();
const deleteImgGa = (value) => {
  files = files.filter((a) => a.name != value.item.alt);
  if (Array.isArray(plugin.value.images)) {
    plugin.value.images = plugin.value.images.filter(
      (a) => a != value.item.alt
    );
  }
  let arr = listThumbnails.value.filter((a) => a != value.item);
   listThumbnails.value=[];
    listThumbnails.value=arr;
};
const listImgPlugin = ref([]);
const displayImgPlugin = ref(false);
const showImgPlugin = (value) => {
  listImgPlugin.value = [];
    let arr = [];
  if(!Array.isArray(value))
  arr=value.split(",");
  else
  arr=value;
  for (let index = 0; index < arr.length; index++) {
    const element = arr[index];
    let image = {};
    image.id = index + 1;
    image.itemImageSrc = baseURL + element;
    image.thumbnailImageSrc = baseURL + element;
    image.alt = element;
    image.title = element;
    listImgPlugin.value.push(image);
  }
  displayImgPlugin.value = true;
};
const responsiveOptions2 = ref([
  {
    breakpoint: "1500px",
    numVisible: 5,
  },
  {
    breakpoint: "1024px",
    numVisible: 3,
  },
  {
    breakpoint: "768px",
    numVisible: 2,
  },
  {
    breakpoint: "560px",
    numVisible: 1,
  },
]);
const isCheckPlugin = ref(false);
const showDetails = (value) => {
  selectedKey.value[keyselected.value] = false;
  if (!value.data.plugin_id) {
    value.key = value.data.category_id;
    selectedKey.value[value.key] = true;

    keyselected.value = value.key;
    datalists.value
      .filter((x) => x.active)
      .forEach(function (d) {
        d.active = false;
      });
    value.data.active = true;
    nodeValue.value = value;
    onNodeSelect(value);
  } else {
    listImgPlugin.value = [];
     if (!Array.isArray(value.data.images))
      value.data.images = value.data.images.split(",");
    for (let index = 0; index < value.data.images.length; index++) {
      const element = value.data.images[index];
      let image = {};
      image.id = index + 1;
      image.itemImageSrc = baseURL + element;
      image.thumbnailImageSrc = baseURL + element;
      image.alt = element;
      image.title = element;
      listImgPlugin.value.push(image);
    }
    if (!Array.isArray(value.data.file_url))
      value.data.file_url = value.data.file_url.split(",");

    value.key = value.data.plugin_name;

    selectedKey.value[value.key] = true;

    keyselected.value = value.key;
    datalists.value
      .filter((x) => x.active)
      .forEach(function (d) {
        d.active = false;
      });
    value.data.active = true;

    plugin.value = value.data;
    console.log("Dữ liệu",plugin.value);
    isCheckPlugin.value = true;
  }
};
onMounted(() => {

  loadProject();
  return {
    datalists,
    options,
    onPage,
    loadCount,
    addPlugin,
    closeDialog,
    basedomainURL,
    handleFileUpload,
    saveData,
    isFirst,
    searchPlugin,
    selectedPlugins,
    deleteList,
  };
});
</script>
<template>
  <div class="surface-100">
    <Splitter class="w-full">
      <SplitterPanel :size="20">
        <div class="m-3 mr-0 flex">
          <div>
            <img
              :src="
                projectLogo
                  ? basedomainURL + projectLogo
                  : '/src/assets/image/noimg.jpg'
              "
              alt=""
              class="p-0 pr-2"
              width="45"
              height="40"
            />
          </div>
          <Dropdown
            v-model="projectSelected"
            :options="listProject"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn dự án"
            class="w-full"
            @change="loadCategory"
          >
          </Dropdown>
          <Button
            class="w-4rem ml-2 p-button-outlined p-button-secondary"
            icon="pi pi-refresh"
            @click="refreshTypeApi"
          />
        </div>

        <div style="height: calc(100vh - 128px)">
          <TreeTable
            :value="listCategory"
            @nodeSelect="onNodeSelect($event, true)"
            @node-unselect="onUnNodeSelect"
            selectionMode="single"
            v-model:selectionKeys="selectedKey"
            class="h-full w-full overflow-x-hidden"
            scrollHeight="flex"
            responsiveLayout="scroll"
            :scrollable="true"
            :expandedKeys="expandedKeys"
          >
            <Column
              field="category_name"
              :expander="true"
              class="cursor-pointer flex"
            >
              <template #header>
                <Toolbar class="w-full p-0 border-none sticky top-0">
                  <template #start>
                    <div class="font-bold text-xl">Loại thư viện</div>
                  </template>
                  <template #end>
                    <div v-if="isTypeAPI">
                      <Button
                        icon="pi pi-plus "
                        class="p-button-success"
                        @click="addCategory('Thêm loại API')"
                      />
                      <Button
                        class="mx-1"
                        v-if="nodeSelected != null"
                        type="button"
                        icon="pi pi-pencil"
                        @click="editCategory(nodeSelected.category_id)"
                      ></Button>
                      <Button
                        icon="pi pi-trash"
                        class="p-button-danger"
                        v-if="nodeSelected != null"
                        @click="deleteCategory(nodeSelected.category_id)"
                      />
                    </div>
                  </template>
                </Toolbar>
              </template>
              <template #body="data">
                <div
                  class="relative flex w-full p-0"
                  v-if="!data.node.data.plugin_id"
                >
                  <div class="grid w-full p-0">
                    <div
                      class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                    >
                      <div class="col-2 p-0">
                        <img
                          src="../../assets/image/folder.png"
                          width="28"
                          height="36"
                          style="object-fit: contain"
                        />
                      </div>
                      <div class="col-10 p-0">
                        <div
                          class="px-2"
                          style="line-height: 36px"
                        >
                          {{ data.node.data.category_name }}
                          <span v-if="data.node.children.length > 0"
                            >({{ data.node.children.length }})</span
                          >
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="relative flex w-full p-0" v-else>
                  <div class="grid w-full p-0">
                    <div
                      class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                    >
                      <div class="col-2 p-0">
                        <!-- <img
                          src="../../assets/image/service.png"
                          class="pr-2 pb-0"
                          width="28"
                          height="36"
                          style="object-fit: contain"
                        /> -->
                        <i class="pi pi-book pt-2"></i>
                      </div>
                      <div class="col-9 p-0">
                        <div style="line-height: 36px">
                          {{ data.node.data.plugin_name }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Column>
          </TreeTable>
        </div>
      </SplitterPanel>
      <SplitterPanel :size="80">
        <div class="d-lang-table">
          <DataView
            class="w-full h-full e-sm flex flex-column"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            :rowsPerPageOptions="[8, 12, 20, 50, 100]"
            currentPageReportTemplate=""
            responsiveLayout="scroll"
            :scrollable="true"
            :layout="layout"
            :lazy="true"
            :value="datalists"
            :loading="options.loading"
            :paginator="options.totalRecords > options.PageSize"
            :rows="options.PageSize"
            :totalRecords="options.totalRecords"
          >
            <template #header>
              <div>
                <h3 class="m-0">
                  <i class="pi pi-book"></i> Danh sách thư viện
                  <span v-if="options.totalRecords > 0"
                    >({{ options.totalRecords }})</span
                  >
                </h3>
                <Toolbar class="w-full custoolbar pt-5">
                  <template #start>
                    <span class="p-input-icon-left mr-2">
                      <i class="pi pi-search" />
                      <InputText
                        type="text"
                        class="p-inputtext-sm"
                        spellcheck="false"
                        placeholder="Tìm kiếm"
                      />
                    </span>
                  </template>

                  <template #end>
                    <DataViewLayoutOptions v-model="layout" class="mr-2" />

                    <Button
                      v-if="nodeSelected"
                      @click="addPlugin('Thêm thư viện')"
                      label="Thêm mới"
                      icon="pi pi-plus"
                      class="p-button-sm mr-2"
                    />
                    <Button
                      class="
                        mr-2
                        p-button-sm p-button-outlined p-button-secondary
                      "
                      @click="refreshService"
                      icon="pi pi-refresh"
                    />
                    <Button
                      label="Tiện ích"
                      icon="pi pi-file-excel"
                      class="mr-2 p-button-outlined p-button-secondary"
                      aria-haspopup="true"
                      aria-controls="overlay_Export"
                    />
                    <Menu id="overlay_Export" ref="projectButs" :popup="true" />
                  </template>
                </Toolbar>
              </div>
            </template>
            <template #grid="slotProps">
              <div class="col-12 md:col-3 p-2">
                <Card class="no-paddcontent">
                  <template #title>
                    <div style="position: relative">
                      <div
                        @click="showDetails(slotProps)"
                        class="cursor-pointer"
                      >
                        <div
                          class="
                            align-items-center
                            justify-content-center
                            text-center
                          "
                        >
                          <Avatar
                            image="./src/assets/image/folder.png"
                            class="mr-2"
                            size="xlarge"
                            shape="square"
                            v-if="!slotProps.data.plugin_id"
                          />
                          <Avatar
                            :image="
                              slotProps.data.images
                                ? basedomainURL + slotProps.data.images[0]
                                : '/src/assets/image/noimg.jpg'
                            "
                            class="mr-2"
                            size="xlarge"
                            shape="square"
                            v-else
                          />
                        </div>
                      </div>
                      <Button
                        style="position: absolute; right: 0px; top: 0px"
                        icon="pi pi-ellipsis-h"
                        class="p-button-rounded p-button-text ml-2"
                        @click="
                          toggleMores(
                            $event,
                            slotProps.data,
                            slotProps.data.project_id ? true : false
                          )
                        "
                        aria-haspopup="true"
                        aria-controls="overlay_More"
                      />
                      <Menu
                        id="overlay_More"
                        ref="menuButMores"
                        :model="itemButMores"
                        :popup="true"
                      />
                    </div>
                  </template>

                  <template #content>
                    <div
                      class="text-center cursor-pointer"
                      @click="showDetails(slotProps)"
                    >
                      <div>
                        <div v-if="slotProps.data.plugin_id">
                          <div
                            class="text-lg text-blue-400 font-bold pb-2"
                            style="word-break: break-all"
                          >
                            {{ slotProps.data.plugin_name }}
                          </div>
                          <div v-html="slotProps.data.title"></div>
                        </div>
                        <div v-else>
                          <div
                            class="mb-1 text-lg text-blue-400 font-bold"
                            style="word-break: break-all"
                          >
                            {{ slotProps.data.category_name }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Card>
              </div>
            </template>
            <template #list="slotProps">
              <div class="w-full" v-if="slotProps.data.plugin_id">
                <div class="flex align-items-center justify-content-center">
                  <div
                    class="
                      flex flex-column flex-grow-1
                      surface-0
                      m-2
                      border-round-xs
                      p-3
                      
                    "
                    :class="'row ' + slotProps.data.active"
                  >
                    <div class="col-12 field flex p-0 m-0 px-2">
                      <div class="col-1 field p-0 m-0">
                        <div v-if="slotProps.data.plugin_id">
                          <img
                            :src="
                              slotProps.data.images
                                ? basedomainURL + slotProps.data.images[0]
                                : '/src/assets/image/noimg.jpg'
                            "
                            alt="imgPlgin"
                            width="100"
                            height="50"
                            style="object-fit: contain"
                            class="cursor-pointer"
                            @click="showImgPlugin(slotProps.data.images)"
                          />
                        </div>
                        <div v-else>
                          <img
                            src="../../assets/image/folder.png"
                            width="28"
                            height="36"
                            style="object-fit: contain"
                          />
                        </div>
                      </div>

                      <div
                        class="col-6 field flex p-0 m-0"
                        @click="showDetails(slotProps)"
                      >
                        <div class="col-8 p-0 cursor-pointer p-0 m-0">
                          <div class="col-12 font-bold text-xl p-0 m-0">
                            <div v-if="slotProps.data.plugin_id">
                              {{ slotProps.data.plugin_name }}
                            </div>
                            <div v-else>
                              <div class="mb-1 font-bold text-xl">
                                {{ slotProps.data.category_name }}
                              </div>
                            </div>
                          </div>
                          <div class="col-12 p-0 m-0">
                            <div>
                              {{ slotProps.data.title }}
                            </div>
                          </div>
                          <div class="col-12 p-0 m-0 flex">
                            <div
                              v-for="(item, idxItem) in slotProps.data.keywords" :key="idxItem"
                              class="mr-1"
                            >
                              <Chip>
                                {{ item }}
                              </Chip>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-5 field p-0 m-0 px-2 pb-2 cursor-pointer">
                        <div class="col-12 p-0 m-0">
                          <div class="flex col-12 p-0 m-0">
                            <div
                              v-if="slotProps.data.created_by"
                              class="col-10 p-0 m-0"
                              @click="showDetails(slotProps)"
                            >
                              <i class="pi pi-user text-color-secondary"></i>
                              by: {{ slotProps.data.created_by }}
                            </div>
                            <div class="col-2 text-right flex">
                              <Toolbar
                                class="
                                  w-full
                                  surface-0
                                  outline-none
                                  border-none
                                  p-0
                                "
                                :class="'row ' + slotProps.data.active"
                              >
                                <template #start> </template>
                                <template #end>
                                  <div>
                                    <Button
                                      icon="pi pi-ellipsis-h"
                                      class="
                                        p-button-outlined p-button-secondary
                                        ml-2
                                        border-none
                                      "
                                      @click="
                                        toggleMores(
                                          $event,
                                          slotProps.data,
                                          slotProps.data.project_id
                                            ? true
                                            : false
                                        )
                                      "
                                      aria-haspopup="true"
                                      aria-controls="overlay_More"
                                    />
                                    <Menu
                                      id="overlay_More"
                                      ref="menuButMores"
                                      :model="itemButMores"
                                      :popup="true"
                                    />
                                  </div>
                                </template>
                              </Toolbar>
                            </div>
                          </div>
                          <div
                            class="flex col-12 p-0"
                            @click="showDetails(slotProps)"
                          >
                            <div class="">
                              <div v-if="slotProps.data.created_date">
                                <i
                                  class="pi pi-calendar text-color-secondary"
                                ></i>
                                {{
                                  moment(
                                    new Date(slotProps.data.created_date)
                                  ).format("DD/MM/YYYY")
                                }}
                              </div>
                            </div>
                            <div class="px-8 pt-2">
                              <i class="pi pi-tags text-color-secondary"></i>
                              {{ categoryName }}
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="w-full" v-else>
                <div class="flex align-items-center justify-content-center">
                  <div
                    class="
                      flex flex-column flex-grow-1
                      surface-0
                      m-2
                      border-round-xs
                      pl-3
                      pt-3
                    "
                    :class="'row ' + slotProps.data.active"
                  >
                    <div class="col-12 field flex p-0 m-0 px-2">
                      <div
                        class="col-8 p-0 cursor-pointer"
                        @click="showDetails(slotProps)"
                      >
                        <div class="col-12 p-0 font-bold text-xl">
                          <div class="mb-1 font-bold text-xl">
                            {{ slotProps.data.category_name }}
                          </div>
                        </div>
                      </div>
                      <div class="col-4 text-right flex">
                        <Toolbar
                          class="w-full surface-0 outline-none border-none p-0"
                          :class="'row ' + slotProps.data.active"
                        >
                          <template #start> </template>
                          <template #end>
                            <div>
                              <Button
                                icon="pi pi-ellipsis-h"
                                class="
                                  p-button-outlined p-button-secondary
                                  ml-2
                                  border-none
                                "
                                @click="
                                  toggleMores(
                                    $event,
                                    slotProps.data,
                                    slotProps.data.project_id ? true : false
                                  )
                                "
                                aria-haspopup="true"
                                aria-controls="overlay_More"
                              />
                              <Menu
                                id="overlay_More"
                                ref="menuButMores"
                                :model="itemButMores"
                                :popup="true"
                              />
                            </div>
                          </template>
                        </Toolbar>
                      </div>
                    </div>
                    <div
                      class="col-12 field flex p-0 m-0 px-2 pb-2 cursor-pointer"
                      @click="showDetails(slotProps)"
                    >
                      <div>
                        <div>
                          <img
                            src="../../assets/image/folder.png"
                            width="28"
                            height="36"
                            style="object-fit: contain"
                          />
                        </div>
                      </div>
                      <div class="pl-8 pt-2">
                        <div v-if="slotProps.data.created_date">
                          <i class="pi pi-calendar text-color-secondary"></i>
                          {{
                            moment(
                              new Date(slotProps.data.created_date)
                            ).format("DD/MM/YYYY")
                          }}
                        </div>
                      </div>
                      <div class="px-8 pt-2">
                        <i class="pi pi-tags text-color-secondary"></i>
                        {{ categoryName }}
                      </div>
                      <div class="px-8 pt-2">
                        <div v-if="slotProps.data.created_by">
                          <i class="pi pi-user text-color-secondary"></i>
                          by: {{ slotProps.data.created_by }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </template>
            <template #empty>
              <div
                class="
                  align-items-center
                  justify-content-center
                  p-4
                  text-center
                "
                v-if="!isFirst"
              >
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataView>
          <div>
            <Galleria
              :value="listImgPlugin"
              :responsiveOptions="responsiveOptions2"
              :numVisible="5"
              containerStyle="max-width: 50%"
              :circular="true"
              :fullScreen="true"
              :showItemNavigators="true"
              v-model:visible="displayImgPlugin"
            >
              <template #item="slotProps">
                <img
                  :src="slotProps.item.itemImageSrc"
                  :alt="slotProps.item.alt"
                  style="
                    width: 100%;
                    display: block;
                    max-width: 1024px;
                    max-height: 964px;
                    object-fit: contain;
                  "
                />
              </template>
              <template #thumbnail="slotProps">
                <img
                  :src="slotProps.item.thumbnailImageSrc"
                  :alt="slotProps.item.alt"
                  style="
                    display: block;
                    width: 150px;
                    height: 100px;
                    object-fit: contain;
                  "
                />
              </template>
            </Galleria>
          </div>
        </div>
      </SplitterPanel>
    </Splitter>
    <Sidebar
      v-model:visible="isCheckPlugin"
      :baseZIndex="100"
      position="right"
      class="p-sidebar-lg"
    >
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <h1>{{ plugin.plugin_name }}</h1>
        </div>
        <div class="field col-12 md:col-12">
          <Button   v-if="plugin.is_app" class="p-0 mr-2" type="button">APP</Button>

          <span style="color: cornflowerblue; fon-size: 14px"
            >{{ plugin.created_by }},</span
          >
          <i style="padding: 0px 12px" class="pi pi-clock"></i
          ><span>Ngày: {{ plugin.created_date }}</span>
        </div>
        <div class="field col-12 md:col-12">
          <hr />
        </div>
        <div class="field col-12 md:col-12">
          <h3>{{ plugin.title }}</h3>
        </div>
        <div class="field col-12 md:col-12 format-center">
          <Galleria
            :value="listImgPlugin"
            :responsiveOptions="responsiveOptions2"
            :numVisible="5"
            containerStyle="max-width: 70%"
            :circular="true"
            :showThumbnails="false"
            :showItemNavigators="true"
          >
            <template #item="slotProps">
              <img
                :src="slotProps.item.itemImageSrc"
                :alt="slotProps.item.alt"
                style="
                  width: 100%;
                  display: block;
                  max-width: 524px;
                  height: 368px;
                  object-fit: contain;
                "
              />
            </template>
           
          </Galleria>
        </div>
        <div class="field col-12 md:col-12 format-center">
          <div class="col-12 p-0 cursor-pointer" v-if="plugin.file_url">
            <div
              v-for="(item, index) in plugin.file_url"
              :key="index"
              class="flex"
            >
            <a :href="basedomainURL + item" download class="w-full no-underline">
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <img
                      src="/src/assets/image/rarimg.png"
                      style="object-fit: contain"
                      width="50"
                      height="50"
                      alt="logorar"
                    />
                    <span style="line-height: 50px">
                      {{ item.substring(16) }}</span
                    >
                  </div>
                </template>
              </Toolbar>
              </a>
            </div>
          </div>
        </div>

        <div class="field col-12 md:col-12">
          <p v-html="plugin.des" style="font-size: 16px"></p>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div v-for="(item, idxItem) in plugin.keywords" :key="idxItem" class="mr-1">
            <Chip>
              {{ item }}
            </Chip>
          </div>
        </div>
      </div>
    </Sidebar>
  </div>
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '50vw' }"
    @hide="reloadPlugin"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Thư viện <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="plugin.plugin_name"
            spellcheck="false"
            class="col-10 ip36 px-2"
            :class="{ 'p-invalid': v$.plugin_name.$invalid && submitted }"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.plugin_name.$invalid && submitted) ||
              v$.plugin_name.$pending.$response
            "
            class="col-9 p-error"
          >
            <span class="col-12 p-0">{{
              v$.plugin_name.required.$message
                .replace("Value", "Tên thư viện")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-8 md:col-8">
          <div class="col-12 field md:col-12 p-0">
            <label class="col-3 text-left">Tiêu đề</label>
            <InputText
              spellcheck="false"
              class="col-9 ip36 p-2"
              v-model="plugin.title"
            ></InputText>
          </div>
          <div class="col-12 p-0 flex field">
            <label class="col-3 text-left">Link</label>
            <InputText
              v-model="plugin.is_url"
              spellcheck="false"
              class="col-9 ip36 px-2 p-2"
            />
          </div>
          <div class="field col-12 md:col-12 flex p-0">
            <label class="col-3 text-left pt-2">Mô tả</label>
            <div class="col-9 p-0">
              <!-- <Textarea v-model="plugin.des" class="col-12 ip36 p-0 m-0 "  rows="6" cols="30" autoResize /> -->

              <Editor
                spellcheck="false"
                v-model="plugin.des"
                editorStyle="height: 120px"
              />
            </div>
          </div>
        </div>
        <div class="field col-4 md:col-4">
          <label class="col-12 text-center p-0 pl-3">Hình ảnh</label>
          <!-- inputanh -->
          <div
            class="
              border-500 border-1 border-solid
              col-12
              p-0
              h-16rem
              mt-1
              relative
            "
            style="background-color: #eeeeee"
          >
            <Galleria
              :showThumbnails="showThumbnails"
              :value="listThumbnails"
              :numVisible="3"
              :activeIndex="activeIndex"
    
            >
              <template #item="slotProps">
                <div  v-if="slotProps.item">
                  <i
                    v-if="slotProps.item.id != 0"
                    class="pi pi-times cursor-pointer absolute top-0 right-0"
                    @click="deleteImgGa(slotProps)"
                  ></i>
                  <img
                    @click="chonanh('AnhTem')"
                    :src="
                      slotProps.item.thumbnailImageSrc
                        ? slotProps.item.thumbnailImageSrc
                        : '/src/assets/image/noimg.jpg'
                    "
                    style="background-color: #eeeeee; object-fit: contain"
                    class="w-full h-12rem"
                  />
                </div>
               
              </template>
              <template #thumbnail="slotProps">
                <img
                  :src="
                    slotProps.item.thumbnailImageSrc
                      ? slotProps.item.thumbnailImageSrc
                      : '/src/assets/image/noimg.jpg'
                  "
                  :alt="slotProps.item.alt"
                  style="object-fit: contain; width: 100%; height: 50px"
                />
              </template>
            </Galleria>
          </div>
          <input
            class="ipnone"
            id="AnhTem"
            type="file"
            multiple="true"
            accept="image/*"
            @change="handleFileUpload"
          />
        </div>

        <div class="col-12 p-0 flex field">
          <label class="col-2 text-left">File</label>
          <div class="col-10 p-0">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="true"
              accept=".zip,.rar"
              :maxFileSize="10000000"
              @select="onUploadFile"
              @remove="removeFile"
            />
          </div>
        </div>
        <div class="col-12 p-0 flex field">
          <label class="col-2 text-left"></label>
          <div class="col-10 p-0" v-if="plugin.file_url">
            <div
              v-for="(item, index) in plugin.file_url"
              :key="index"
              class="flex"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <img
                      src="/src/assets/image/rarimg.png"
                      style="object-fit: contain"
                      width="50"
                      height="50"
                      alt="logorar"
                    />
                    <span style="line-height: 50px">
                      {{ item.substring(16) }}</span
                    >
                  </div>
                </template>
                <template #end>
                  <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileCode(item)"
                  />
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 p-0 flex">
          <div class="col-4 field md:col-4 p-0">
            <label class="col-6 text-left">STT</label>
            <InputNumber v-model="plugin.is_order" class="col-6 ip36 p-0" />
          </div>
          <div class="col-4 flex pt-1">
            <label style="vertical-align: text-bottom" class="col-6 text-center"
              >Trạng thái
            </label>
            <InputSwitch v-model="plugin.status" class="col-6 ml-1" />
          </div>

          <div class="col-4 p-0 flex pt-1">
            <label style="vertical-align: text-bottom" class="col-5 text-center"
              >App
            </label>
            <InputSwitch v-model="plugin.is_app" class="col-6" />
          </div>
        </div>
        <div class="col-6 flex p-0"></div>

        <div class="field col-12 md:col-12 p-0">
          <label class="col-2 text-left">Từ khóa</label>
          <Chips
            v-model="plugin.keywords"
            spellcheck="false"
            class="col-10 ip36 p-0"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
        autofocus
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerCate"
    v-model:visible="displayCate"
    :style="{ width: '40vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div v-if="isChirlden" class="field col-12 md:col-12 pb-2">
          <label class="col-3 text-left p-0"
            >Cấp cha<span class="redsao"></span
          ></label>
          <InputText
            spellcheck="false"
            v-model="nameParent"
            :disabled="true"
            class="col-8 ip36 px-2"
          />
        </div>

        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Tên loại Api <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="category.category_name"
            spellcheck="false"
            class="col-8 ip36 px-2"
            :class="{
              'p-invalid': validatePlugin.category_name.$invalid && submitted,
            }"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (validatePlugin.category_name.$invalid && submitted) ||
              validatePlugin.category_name.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validatePlugin.category_name.required.$message
                .replace("Value", "Tên loại API")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div style="display: flex" class="col-12 field md:col-12">
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">Số thứ tự </label>
            <InputNumber v-model="category.is_order" class="col-6 ip36 p-0" />
          </div>
          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Trạng thái
            </label>
            <InputSwitch v-model="category.status" class="col-6" />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeCategory"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveCategory(!validatePlugin.$invalid)"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 55px);
}
.inputanh {
  border: 1px solid #ccc;
  width: 224px;
  height: 128px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-galleria-content) {
  .p-galleria-item-wrapper {
    height: 100%;
  }
  .p-galleria-thumbnail-container {
    padding: 4px 2px;
    background-color: rgb(195, 195, 195);
  }
  .p-galleria-thumbnail-next {
    display: block;
  }
  .p-galleria-thumbnail-prev {
    display: block;
  }
}
.row.true {
  background-color: rgb(190, 211, 245) !important;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }
}
</style>