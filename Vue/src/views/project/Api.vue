<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import vi from "date-fns/locale/vi";

import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
//Khởi tạo của H
const DrdMethod = ref([
  { label: "HttpGet", value: "HttpGet" },
  { label: "HttpPost", value: "HttpPost" },
  { label: "HttpPut", value: "HttpPut" },
  { label: "HttpDelete", value: "HttpDelete" },
]);
const check = ref(false);
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const isFirst = ref(true);
const basedomainURL = baseURL;

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref();
const options = ref({
  IsNext: true,
  sort: "project_id",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  FilterUsers_ID: null,
  loading: true,
  totalRecords: null,
  searchTextBug: null,
});
const bug = ref({
  bug_name: "",
  des: "",
  status: 0,
  keyword: "",
});
const listProject = ref([]);
const projectSelected = ref();
const listCategory = ref([]);
const database_name = ref();
watch(projectSelected, () => {
  listProject.value.forEach((element) => {
    if (element.code == projectSelected.value) {
      projectLogo.value = element.project_logo;
      database_name.value = element.db_name;
    }
  });
});
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
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });

    listCategory.value = [];
    listCategorySave.value = [];
    let listCate = [];
    let listSer = [];
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
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_bug_listall",
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listBugs.value = data;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_service_list_all",
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
          if (!element.checkbug) element.checkbug = 0;
          listBugs.value.forEach((item) => {
            if (element.service_id == item.service_id && item.status == 2)
              element.checkbug = 2;
          });
          listBugs.value.forEach((item) => {
            if (element.service_id == item.service_id && item.status < 0)
              element.checkbug = 1;
          });
        });

        listSer = data;
        listService.value = data;
        options.value.loading = false;
      })
      .catch((error) => {
        console.log(error);
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_parameter_list_all",
          par: [
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listParameters.value = data;
        renderService(listCate, listSer, data);
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_table_list",
          par: [
            { par: "db_name", va: database_name.value },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          listTable.value.push({
            name: element.table_name,
            code: element.table_id,
          });
        });
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const listBugs = ref();
const listParameters = ref();

const listStatus = ref([
  {
    name: "Lỗi",
    code: -2,
  },
  {
    name: "Đã sửa",
    code: 1,
  },
  {
    name: "Đang sửa",
    code: -1,
  },
  {
    name: "Đã đóng",
    code: 2,
  },
]);
function checkCsharpType(ptype) {
  let arr = CsharpType.value.filter((x) => x.name == ptype);
  if (arr.length > 0) return false;
  return true;
}
const CsharpType = ref([
  {
    name: "object",
  },
  {
    name: "string",
  },
  {
    name: "int",
  },
  {
    name: "datetime",
  },
  {
    name: "byte",
  },
  {
    name: "float",
  },
  {
    name: "double",
  },
  {
    name: "decimal",
  },
  {
    name: "bool",
  },
  {
    name: "char",
  },
]);
const listTable = ref([]);
const listService = ref([]);
const listCategorySave = ref([]);
const treeCateSelect = ref();
const loadCategory = () => {
  let listCate = [];
  let listSer = [];
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
        //console.log("List CATE", data);
        listCategorySave.value = data;
        listCate = data;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_service_list_all",
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

        listService.value = data;
        listSer = data;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_parameter_list_all",
          par: [
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        renderService(listCate, listSer, data);
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const count = ref();
const Listcount = ref([]);
const getcount = (data) => {
  let id = data;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_service_count",
        par: [{ par: "category_id", va: data }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      count.value = data[0].count;
      //console.log(count.value, "c");
      let count1 = { key: id, count: count.value };
      Listcount.value.push(count1);
      //console.log(Listcount.value, "l");
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const renderService = (listCate, listSer, listParam) => {
  listSer.forEach((element) => {
    if (element.keywords != null && element.keywords.length > 1) {
      if (!Array.isArray(element.keywords)) {
        element.keywords = element.keywords.split(",");
      }
    }
  });
  Listcount.value = [];
  count.value = null;
  let arrChils = [];
  listCate
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      getcount(m.category_id);
      let om = { key: m.category_id, data: m };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        let dts = listCate.filter((x) => x.parent_id == category_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            getcount(em.category_id);
            let om1 = { key: em.category_id, data: em };
            rechildren(om1, em.category_id);
            mm.children.push(om1);
          });
        }
        if (listSer.length > 0) {
          let dsv = listSer.filter((x) => x.category_id == category_id);
          if (dsv.length > 0) {
            dsv.forEach((em) => {
              let om1 = { key: em.service_name, data: em };
              if (listParam.length > 0) {
                let dsp = listParam.filter(
                  (x) => x.service_id == em.service_id
                );
                if (dsp.length > 0) {
                  dsp.forEach((dspm) => {
                    let om2 = { key: dspm.parameters_name, data: dspm };
                    if (!om1.children) om1.children = [];
                    om1.children.push(om2);
                  });
                }
              }
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

const renderCategory = (listCate, category_Id, check) => {
  let arrChils = [];
  if (check)
    arrChils.push({ key: 0, label: "Không có cha", data: null, children: [] });
  listCate
    .filter((x) => x.parent_id == null && x.category_id != category_Id)
    .forEach((m) => {
      let om = {
        key: m.category_id,
        label: m.category_name,
        data: m.category_id,
      };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        let dts = listCate.filter(
          (x) => x.parent_id == category_id && x.category_id != category_Id
        );
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = {
              key: em.category_id,
              label: em.category_name,
              data: em.category_id,
            };

            rechildren(om1, em.category_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.category_id);
      arrChils.push(om);
    });
  //   arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn Module----" });
  treeCateSelect.value = arrChils;
};
const datalists = ref([]);
const dataListsParam = ref([]);
const checkNode = ref(false);
function isJson(str) {
  try {
    JSON.parse(str);
  } catch (e) {
    return false;
  }
  return true;
}
const onUnNodeSelect = () => {
  nodeSelected.value = null;
  isChirlden.value = false;
  checkNode.value = false;
  datalists.value = [];
  dataListsParam.value = [];
};
const isCheckParam = ref(false);
const nodeSelected = ref();
const nodeValue = ref();
const isTypeAPI = ref(true);
const selectedService_Id = ref();
const categoryName = ref();
const headerParam = ref("");
const categoryIdSave = ref();
const dataService = ref([]);
const sttParam = ref(1);
const keyJsonData = ref([]);
const deleteFileCode = () => {
  service.value.url_file = "";
};
const deleteFileBug = () => {
  Bug.value.url_file = "";
};
const onNodeSelect = (node) => {
  keyselected.value = node.key;
  if (selectedKey.value) {
    selectedKey.value[keyselected.value] = false;
    selectedKey.value[node.key] = true;
  }
  if (expandedKeys.value[node.key] == true) {
    expandedKeys.value[node.key] = false;
  } else {
    expandedKeys.value[node.key] = true;
  }
  checkNode.value = true;
  nodeValue.value = node;
  options.value.loading = true;
  categoryName.value = node.data.category_name;
  datalists.value.forEach(function (d) {
    if (d.service_id == node.data.service_id) {
      d.active = true;
    } else {
      d.active = false;
    }
  });
  if (node.data.service_id) {
    let strNode = "";
    if (node.data.data) strNode = node.data.data.replace(/<br\/>|<br>|\n/, "");
    dataService.value = [];
    keyJsonData.value = [];

    if (isJson(strNode)) {
      try {
        strNode = JSON.parse(strNode);
        let checkarr = false;
        strNode.forEach((element) => {
          if (checkarr && !Array.isArray(element)) return;
          checkarr = true;
          let arrkey = [];
          let checkL2 = false;
          for (var i in element) {
            if (checkL2 && !Array.isArray(element[i])) break;
            else {
              if (Array.isArray(element)) {
                checkL2 = true;

                var val = element[i];

                for (var j in val) {
                  var sub_key = j;
                  arrkey.push({ key: sub_key, value: val[j] });
                }
              } else {
                arrkey.push({ key: i, value: element[i] });
              }
            }
          }
          dataService.value.push({
            des: "",
            data: "[" + JSON.stringify(element) + "]",
          });
          keyJsonData.value.push(arrkey);
        });
      } catch (error) {
        swal
          .fire({
            title: "Thông báo",
            text: "Dữ liệu trả về không đúng định dạng !" + error,
            icon: "warning",

            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Ok",
          })
          .then((result) => {
            return;
          });
        return;
      }
    } else {
      let dataSave = node.data.data;
      if (dataSave) {
        let count = (dataSave.match(/{/g) || []).length;
        for (let index = 0; index < count; index++) {
          if (dataSave.substring(0, 2) == ",{") break;
          let arrkey = [];
          let start = dataSave.indexOf("{");
          let end = dataSave.indexOf("}");
          if (dataSave.indexOf("[[") == -1) {
            dataService.value.push({
              des: start != 0 ? dataSave.slice(0, start).replace("[", " ") : "",
              data: "[" + dataSave.slice(start, end + 1) + "]",
            });
          } else {
            dataService.value.push({
              des: start != 0 ? dataSave.slice(0, start).replace("[", " ") : "",
              data: "[[" + dataSave.slice(start, end + 1) + "]]",
            });
          }
          if (isJson("[" + dataSave.slice(start, end + 1) + "]")) {
            var jsonData = JSON.parse(
              "[" + dataSave.slice(start, end + 1) + "]"
            );

            for (var i in jsonData) {
              var val = jsonData[i];
              for (var j in val) {
                var sub_key = j;
                arrkey.push({ key: sub_key, value: val[j] });
              }
            }
            keyJsonData.value.push(arrkey);
          } else {
            swal
              .fire({
                title: "Thông báo",
                text: "Dữ liệu trả về không đúng định dạng !",
                icon: "warning",

                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: "Ok",
              })
              .then((result) => {
                return;
              });
            return;
            //
          }
          dataSave = dataSave.substring(end + 1);
        }
      }
    }

    headerParam.value = node.data.service_name;
    isTypeAPI.value = false;
    dataListsParam.value = [];
    selectedService_Id.value = node.data.service_id;
    isCheckParam.value = true;
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_parameter_list",
          par: [
            { par: "service_id", va: node.data.service_id },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        sttParam.value = 1;
        if (data.length > 0) {
          data.forEach((element, i) => {
            element.is_order = i + 1;
            i++;
          });
          renderTree(data);
        } else {
          dataListsParam.value = [];
        }
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  } else {
    if (node.data.category_id == categoryIdSave.value) {
      return;
    } else {
      if (node.data.parent_id == null) {
        checkParent.value = false;
      } else {
        checkParent.value = true;
      }
      isCheckParam.value = false;
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
              listService.value.forEach((item) => {
                if (element.category_id == item.category_id) arr.push(item);
              });
            });
            listService.value.forEach((item) => {
              if (item.category_id == node.data.category_id) arr.push(item);
            });
            function compare(a, b) {
              // Sử dụng toUpperCase() để chuyển các kí tự về cùng viết hoa
              var typeA = a.created_date;
              var typeB = b.created_date;

              let comparison = 0;
              if (typeA < typeB) {
                comparison = 1;
              } else if (typeA > typeB) {
                comparison = -1;
              }
              return comparison;
            }

            arr = arr.sort(compare);

            arr.forEach((element) => {
              if (!element.parameters) element.parameters = [];
              else element.parameters = [];
              listParameters.value.forEach((item) => {
                if (item.service_id == element.service_id) {
                  element.parameters.push(item);
                }
              });

              if (element.keywords != null && element.keywords.length > 1) {
                if (!Array.isArray(element.keywords)) {
                  element.keywords = element.keywords.split(",");
                }
              }
              datalists.value.push(element);
            });
            datalistSave.value = datalists.value;
          })
          .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;

            if (error && error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
              store.commit("gologout");
            }
          });
        options.value.totalRecords = datalists.value.length;
      })();
    }
  }
};

const renderTree = (data) => {
  let arrChils = [];

  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.is_order = i + 1;
      sttParam.value = m.is_order + 1;
      let om = { key: m.parameters_id, data: m };
      const rechildren = (mm, parameters_id) => {
        let dts = data.filter((x) => x.parent_id == parameters_id);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, j) => {
            let om1 = { key: em.parameters_id, data: em };
            om1.data.is_order = j + 1;
            rechildren(om1, em.parameters_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.parameters_id);

      arrChils.push(om);
    });

  dataListsParam.value = arrChils;
};
const expandedKeys = ref({});
const layout = ref("list");
const menuButMores = ref();
const menuButBugsMores = ref();
const checkCateEdit = ref();
const toggleBugsMores = (event, u) => {
  Bug.value = u;
  menuButBugsMores.value.toggle(event);
};
const toggleMores = (event, u, check) => {
  if (check) {
    category.value = u;
    checkCateEdit.value = true;
  } else {
    service.value = u;
    checkCateEdit.value = false;
  }

  menuButMores.value.toggle(event);
};
const changeCateSelect = (event) => {
  for (var j in dropdownSel.value) {
    if (j == 0) category.value.parent_id = null;
    else category.value.parent_id = j;
  }
};
const changeSerSelect = (event) => {
  for (var j in dropdownSel.value) {
    if (j == 0) service.value.category_id = null;
    else service.value.category_id = j;
  }
};

const itemButBugsMores = ref([
  {
    label: "Sửa",
    icon: "pi pi-cog",
    command: (event) => {
      editBug(Bug.value);
    },
  },

  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteBug(Bug.value);
    },
  },
]);
const itemButMores = ref([
  {
    label: "Sửa",
    icon: "pi pi-cog",
    command: (event) => {
      if (checkCateEdit.value) {
        editCategory(category.value.category_id);
      } else {
        editService(service.value);
      }
    },
  },

  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      if (checkCateEdit.value) {
        deleteCategory(category.value.category_id);
      } else {
        deleteService(service.value.service_id);
      }
    },
  },
]);
const isChirlden = ref(false);
const nameParent = ref();

const rules = {
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
const ruleService = {
  service_name: {
    required,
    $errors: [
      {
        $property: "service_name",
        $validator: "required",
        $message: "Tên API không được để trống!",
      },
    ],
  },
};
const ruleParameter = {
  parameters_name: {
    required,
    $errors: [
      {
        $property: "parameters_name",
        $validator: "required",
        $message: "Tên tham số không được để trống!",
      },
    ],
  },
  parameters_type: {
    required,
    $errors: [
      {
        $property: "parameters_type",
        $validator: "required",
        $message: "Kiểu dữ liệu không được để trống!",
      },
    ],
  },
};
const ruleBug = {
  bug_name: {
    required,
    $errors: [
      {
        $property: "bug_name",
        $validator: "required",
        $message: "Tên lỗi không được để trống!",
      },
    ],
  },
};
const category = ref({
  category_name: "",
  is_order: 1,
  status: true,
});
const service = ref({
  service_name: "",
  des: "",
  is_app: false,
  is_order: 1,
  status: "",
  data: null,
  keywords: "",
  procname: "",
  method: null,
  isFormData: false,
});
const parameter = ref({
  parameters_name: "",
  parameters_type: "",
  des: "",
  example_value: "",
  is_order: "",
  status: false,
  table_id: null,
});
const validateService = useVuelidate(ruleService, service);
const validateParameter = useVuelidate(ruleParameter, parameter);
const validateBug = useVuelidate(ruleBug, bug);
const v$ = useVuelidate(rules, category);
const headerDialog = ref();
const displayBasic = ref();
const submitted = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  headerDialog.value = str;
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
  if (category.value.parent_id == null) {
    dropdownSel.value = { 0: true };
  }
  isChirlden.value = false;

  isSaveCategory.value = false;
  displayBasic.value = true;
};
const closeDialog = () => {
  category.value = ref({
    category_name: "",
    is_order: 1,
    status: true,
  });
  displayBasic.value = false;
};
const editCategory = (value) => {
  renderCategory(listCategorySave.value, value, true);
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
      isChirlden.value = true;
      category.value = data[0];
      if (category.value.parent_id == null) {
        dropdownSel.value = { 0: true };
      } else {
        dropdownSel.value = { [category.value.parent_id]: true };
      }
      headerDialog.value = "Sửa loại API";
      isSaveCategory.value = true;
      displayBasic.value = true;
    });
};
const deleteCategory = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá loại API này không!",
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
              toast.success("Xoá loại API thành công!");
              nodeSelected.value = null;
              reloadService(true);
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const isSaveService = ref(false);
const isSaveCategory = ref(false);
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
          reloadService(true);
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
      .put(
        baseURL + "/api/api_category/Update_category",
        category.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa loại API thành công!");
          reloadService(true);
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
const refreshTypeApi = () => {
  options.value.loading = true;
  loadProject();
  onNodeSelect(nodeValue.value);
};
const selectedKey = ref();
const headerAPI = ref();
const displayAPI = ref(false);
const addService = () => {
  submitted.value = false;
  headerAPI.value = "Thêm API";
  let sttSer;
  if (nodeSelected.value) {
    let stt = 0;
    listService.value.forEach((element) => {
      if (element.category_id == nodeSelected.value.category_id) {
        stt++;
      }
    });
    sttSer = stt + 1;
  }
  service.value = {
    service_name: "",
    des: "",
    is_app: false,
    is_order: sttSer,
    category_id: nodeSelected.value.category_id,
    status: true,
    method: null,
    isFormData: false,
  };
  if (service.value.parent_id == null) {
    dropdownSel.value = { 0: true };
  }
  isSaveService.value = false;
  displayAPI.value = true;
};
const closeAPI = () => {
  service.value = {
    service_name: "",
    des: "",
    is_app: false,
    is_order: 1,
    category_id: null,
    method: null,
    isFormData: false,
  };
  displayAPI.value = false;
};
const reloadParam = (service_id) => {
  options.value.loading = true;

  dataListsParam.value = [];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_parameter_list",
        par: [
          { par: "service_id", va: service_id },
          { par: "search", va: options.value.SearchText },
          { par: "status", va: options.value.Status },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        element.is_order = i + 1;
        i++;
      });
      renderTree(data);

      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const reloadService = (check) => {
  if (check) {
    datalists.value = datalistSave.value;
  } else {
    options.value.loading = true;
    datalists.value = [];
    (async () => {
      await axios
        .post(
          baseURL + "/api/Proc/CallProc",
          {
            proc: "api_service_list",
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
          data.forEach((element) => {
            if (!element.parameters) element.parameters = [];
            listParameters.value.forEach((item) => {
              if (item.service_id == element.service_id) {
                element.parameters.push(item);
              }
            });
            if (element.keywords != null && element.keywords.length > 1) {
              if (!Array.isArray(element.keywords)) {
                element.keywords = element.keywords.split(",");
              }
            }
            datalists.value.push(element);
          });
          console.log("S2", datalists.value);
          datalistSave.value = datalists.value;
          options.value.loading = false;
        })
        .catch((error) => {
          console.log(error);
          toast.error("Tải dữ liệu không thành công!");
          options.value.loading = false;

          if (error && error.status === 401) {
            swal.fire({
              title: "Error!",
              text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
              icon: "error",
              confirmButtonText: "OK",
            });
            store.commit("gologout");
          }
        });
      options.value.totalRecords = datalists.value.length;
    })();
  }
};
const saveAPI = (isFormValid) => {
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url", file);
  }
  if (service.value.des)
    service.value.des = service.value.des.replace("\n", "<br>");
  if (service.value.data)
    service.value.data = service.value.data.replace("\n", "<br>");
  submitted.value = true;
  if (service.value.keywords != null) {
    service.value.keywords = service.value.keywords.toString();
  }
  formData.append("service", JSON.stringify(service.value));

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveService.value) {
    axios
      .post(baseURL + "/api/api_category/Add_service", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm API thành công!");
          files = [];
          loadProject();
          closeAPI();
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
      .put(baseURL + "/api/api_category/Update_service", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật API thành công!");
          files = [];
          loadProject();
          // reloadService();
          closeAPI();
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
const editService = (data) => {
  renderCategory(listCategorySave.value, 0, false);
  isSaveService.value = true;
  submitted.value = false;
  headerAPI.value = "Sửa API";
  if (data.des) data.des = data.des.replace("<br>", "\n");
  if (data.data) data.data = data.data.replace("<br>", "\n");
  if (service.value.category_id == null) {
    dropdownSel.value = { 0: true };
  } else {
    dropdownSel.value = { [service.value.category_id]: true };
  }
  if (data.keywords != null && data.keywords.length > 1) {
    if (!Array.isArray(data.keywords)) {
      data.keywords = data.keywords.split(",");
    }
  }
  service.value = data;
  displayAPI.value = true;
};
const deleteService = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá API này không!",
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
          .delete(baseURL + "/api/api_category/Delete_service", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value != null ? [value] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá API thành công!");
              reloadService(false);
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const displayParameter = ref();
const headerParameter = ref(false);
const addParameter = (parent_id) => {
  submitted.value = false;
  parameter.value = {
    service_id: selectedService_Id.value,
    parameters_name: "",
    parameters_type: "",
    des: "",
    example_value: "",
    is_order: sttParam.value,
    status: true,
    table_id: null,
    parent_id: parent_id,
  };

  headerParameter.value = "Thêm tham số";

  isSaveParameter.value = false;
  displayParameter.value = true;
};

const editParameter = (data) => {
  data.des = data.des.replace("<br>", "\n");
  parameter.value = data;
  headerParameter.value = "Sửa tham số";
  isSaveParameter.value = true;
  displayParameter.value = true;
};
const closeParameter = () => {
  parameter.value = {
    service_id: selectedService_Id.value,
    parameters_name: "",
    des: "",
    example_value: "",
    is_order: 1,
    status: false,
    table_id: null,
  };
  displayParameter.value = false;
};
const closeBug = () => {
  bug.value = {
    bug_name: "",
    des: "",
    status: 0,
    keyword: "",
  };
  isShowAddBug.value = false;
};
const isSaveBug = ref(false);
const saveBug = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  let formData = new FormData();
  for (var i = 0; i < filebugs.length; i++) {
    let file = filebugs[i];
    formData.append("url", file);
  }
  if (bug.value.des) bug.value.des = bug.value.des.replace("\n", "<br>");
  submitted.value = true;
  if (bug.value.keyword != null) {
    bug.value.keyword = bug.value.keyword.toString();
  }
  formData.append("bug", JSON.stringify(bug.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveBug.value) {
    axios
      .post(baseURL + "/api/api_bug/Add_bug", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm lỗi thành công!");

          showBugs(bug.value.service_id);
          closeBug();
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
      .put(baseURL + "/api/api_bug/Update_bug", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật lỗi thành công!");
          showBugs(bug.value.service_id);
          closeBug();
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

const isSaveParameter = ref(false);
const saveParameter = (isFormValid) => {
  parameter.value.des = parameter.value.des.replace("\n", "<br>");
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
  if (!isSaveParameter.value) {
    axios
      .post(
        baseURL + "/api/api_category/Add_parameter",
        parameter.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm tham số thành công!");
          reloadParam(parameter.value.service_id);
          loadProject();
          closeParameter();
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
        baseURL + "/api/api_category/Update_parameter",
        parameter.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật tham số thành công!");
          reloadParam(parameter.value.service_id);
          loadProject();
          closeParameter();
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
const deleteParameter = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tham số này không!",
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
          .delete(baseURL + "/api/api_category/Delete_parameter", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.parameters_id != null ? [value.parameters_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá tham số thành công!");
              reloadParam(value.service_id);
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const keyselected = ref();
const showDetails = (value) => {
  selectedKey.value[keyselected.value] = false;
  if (value.data.project_id) {
    value.key = value.data.category_id;
    selectedKey.value[value.key] = true;
    keyselected.value = value.key;
  } else {
    value.key = value.data.service_name;

    selectedKey.value[value.key] = true;
    keyselected.value = value.key;
  }
  datalists.value
    .filter((x) => x.active)
    .forEach(function (d) {
      d.active = false;
    });
  value.data.active = true;
  nodeValue.value = value;
  onNodeSelect(value);
};
const showDetailsKey = (value) => {
  listDataKey.value
    .filter((x) => x.active)
    .forEach(function (d) {
      d.active = false;
    });
  value.data.active = true;
  nodeValue.value = value;
  onNodeSelect(value);
};
const refreshService = () => {
  if (nodeValue.value) {
    onNodeSelect(nodeValue.value);
  }
};
const parentID = ref();
const checkParent = ref();
const preFolder = () => {
  if (nodeValue.value.data.parent_id) {
    keyselected.value = nodeValue.value.key;
    parentID.value = nodeValue.value.data.parent_id;
    checkParent.value = true;
  }
  listCategorySave.value.forEach((element) => {
    if (element.category_id == parentID.value) {
      onNodeSelect({ key: element.category_id, data: element });
    }
  });
};
const dropdownSel = ref();
//Xuất excelserviceButs
const serviceButs = ref();
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
      exportData("ExportExcel");
    },
  },
]);
const toggleExport = (event) => {
  serviceButs.value.toggle(event);
};
const exportData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (nodeValue.value != null) {
    axios
      .post(
        baseURL + "/api/Excel/ExportExcel",
        {
          excelname: "DANH SÁCH API",
          proc: "api_service_listexport",
          par: [{ par: "Category_id", va: nodeValue.value.data.category_id }],
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
          store.commit("gologout");
        }
      });
  }
};
//

const paramButs = ref();
const itemParamButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportParamData("ExportExcel");
    },
  },
  {
    label: "Import Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportParamData("ExportExcel");
    },
  },
]);
const toggleParamExport = (event) => {
  paramButs.value.toggle(event);
};
const exportParamData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (selectedService_Id.value != null) {
    axios
      .post(
        baseURL + "/api/Excel/ExportExcel",
        {
          excelname: "DANH SÁCH THAM SỐ",
          proc: "api_parameter_listexport",
          par: [{ par: "service_id", va: selectedService_Id.value }],
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
          store.commit("gologout");
        }
      });
  }
};
const stkLength=ref(0);
const isShowSearhKey = ref(false);
const listKeyWords = ref([]);
const showSearchKey = () => {
  listKeyWords.value = [];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_keyservice_list",
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let strs = "";
      data.forEach((element) => {
        if (element.keywords != null) {
          let arr = element.keywords.split(",");
          // if(arr.length>0){
          //   stkLength.value=arr.length;
          // }
          // for (let index = 0; index <  stkLength.value; index++) {
          //   const item = { key: arr[index], active: false };
          //   if (!strs.includes(arr[index])) listKeyWords.value.push(item);
          // }
          if (arr.length > 0) {
            arr.forEach((el, idx) => {
              const item = { key: el, active: false };
              if (!strs.includes(el)) listKeyWords.value.push(item);
            });
          }
          strs += element.keywords + ",";
        }
      });
    });
  isShowSearhKey.value = true;
};
const keyWords = ref([]);
const listDataKey = ref();
const searchKey = (value) => {
  listKeyWords.value.forEach((element) => {
    if (element == value) {
      if (element.active == true) {
        element.active = false;
        value.active = false;
      } else {
        element.active = true;
        value.active = true;
      }
    }
  });
  if (value.active == true) keyWords.value.push(value);
  else keyWords.value = keyWords.value.filter((a) => a != value);
  let strKey = "";
  let detached = "";
  keyWords.value.forEach((element) => {
    strKey += detached + element.key;
    detached = ",";
  });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_service_listwithkey",
        par: [{ par: "keywords", va: strKey }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listDataKey.value = data;
    });
};
const projectLogo = ref();
const keySearch = ref("");
const datalistSave = ref();
const searchAPI = () => {
  datalists.value = datalistSave.value.filter(
    (x) =>
      x.service_name.toLowerCase().search(keySearch.value.toLowerCase()) != -1
  );
};
let files = [];
let filebugs = [];
const onUploadFile = (event) => {
  if (event.files.length > 0) service.value.url_file = "";
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = [];
};

const onUploadFileBug = (event) => {
  if (event.files.length > 0) bug.value.url_file = "";
  event.files.forEach((element) => {
    filebugs.push(element);
  });
};
const removeFileBug = (event) => {
  filebugs = [];
};
const isShowBug = ref(false);
const listBugSave = ref();
const serviceName = ref();
const serviceId = ref();
const showBugs = (value) => {
  serviceName.value = [];
  serviceName.value = listService.value.filter((x) => x.service_id == value);
  serviceName.value = serviceName.value[0].service_name;
  serviceId.value = value;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_bug_list_api",
        par: [
          { par: "service_id", va: value },
          { par: "search", va: options.value.searchTextBug },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        if (element.created_date)
          element.created_date = new Date(
            moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
          );
        else
          element.created_date = new Date(
            moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
          );
      });
      listBugSave.value = data;
      isDetailsBug.value = false;
      isShowBug.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const Bug = ref({
  bug_name: "",
  created_by: "",
  created_date: null,
  des: "",
  keyword: "",
  url_file: "",
  status: 0,
  date_now: "",
});
const isDetailsBug = ref(false);
const reloadComment = (id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [{ par: "bug_id", va: id },  { par: "commentbug_id",va:null }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        if (element.url_file) {
          element.url_file = element.url_file.split(",");
          let arrFile = [];
          let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
          element.url_file.forEach((element) => {
            //Kiểm tra định dạng
            if (allowedExtensions.exec(element)) {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: true,
              });
              URL.revokeObjectURL(element);
            } else {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: false,
              });
            }
          });
          element.url_file = arrFile;
        }
        if (element.created_date)
          element.created_date = new Date(
            moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
          );
        else
          element.created_date = new Date(
            moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
          );
      });
      listCommentBugSave.value = data;

      renderComment(data);
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const reloadChildComment = (comment_id) => {
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_list",
          par: [{ par: "bug_id", va: Bug.value.bug_id },  { par: "commentbug_id",va:null }],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );
        });
        listCommentBugSave.value = data;
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_listwithid",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "parent_id", va: comment_id },   { par: "bugcomment_id", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );

          listCommentBugSave.value.push(element);
        });
        renderComment(listCommentBugSave.value);
        checkAddChildCmt.value = comment_id;
        //Cần làm cái Render Child ở đây
      })
      .catch((error) => {
        console.log(error);
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const showCommentBug = (value) => {
  isDetailsBug.value = true;
  Bug.value = value;
  Bug.value.created_date = new Date(
    moment(Bug.value.created_date).format("YYYY/MM/DD HH:mm:ss")
  );
  if (Bug.value.keyword)
    if (!Array.isArray(Bug.value.keyword))
      Bug.value.keyword = Bug.value.keyword.split(",");
  if (Bug.value.url_file)
    if (!Array.isArray(Bug.value.url_file))
      Bug.value.keyword = Bug.value.url_file.split(",");
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [{ par: "bug_id", va: Bug.value.bug_id },  { par: "commentbug_id",va:null }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        if (element.url_file) {
          element.url_file = element.url_file.split(",");
          let arrFile = [];
          let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
          element.url_file.forEach((element) => {
            //Kiểm tra định dạng
            if (allowedExtensions.exec(element)) {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: true,
              });
              URL.revokeObjectURL(element);
            } else {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: false,
              });
            }
          });
          element.url_file = arrFile;
        }
        if (element.created_date)
          element.created_date = new Date(
            moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
          );
        else
          element.created_date = new Date(
            moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
          );
      });
      listCommentBugSave.value = data;

      renderComment(data);
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const preListBug = () => {
  isDetailsBug.value = false;
};
const listCommentBugs = ref();
const listCommentBugSave = ref();
const comment = ref();
const addComment = (check, comment_id) => {
  if (check) {
    if (comment.value == "" || comment.value == null) return;
    else {
      let bugComment = {
        bug_id: Bug.value.bug_id,
        des: comment.value,
      };
      let formData = new FormData();
      for (var i = 0; i < filecoments.length; i++) {
        let file = filecoments[i];
        formData.append("url_file", file);
      }
      filecoments = [];
      listFileComment.value = [];
      formData.append("comment", JSON.stringify(bugComment));
      comment.value = "";
      axios
        .post(baseURL + "/api/api_commentbug/Add_commentbug", formData, config)
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
            toast.success("Thêm bình luận thành công!");
            isDetailsBug.value = true;
            reloadComment(Bug.value.bug_id);
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
  } else if (commentChild.value == "" || commentChild.value == null) return;
  else {
    let bugComment = {
      bug_id: Bug.value.bug_id,
      parent_id: comment_id,
      des: commentChild.value,
    };
    commentChild.value = "";
    let formData = new FormData();
    for (var i = 0; i < filecoments.length; i++) {
      let file = filecoments[i];
      formData.append("url_file", file);
    }
    filecoments = [];
    listFileCommentChild.value = [];
    formData.append("comment", JSON.stringify(bugComment));
    commentChild.value = "";

    axios
      .post(baseURL + "/api/api_commentbug/Add_commentbug", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");
          isDetailsBug.value = true;
          reloadChildComment(comment_id);
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

const deleteComment = (value, check) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bình luận này không!",
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
          .delete(baseURL + "/api/api_commentbug/Delete_commentbug", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.comment_id != null ? [value.comment_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bình luận thành công!");
              if (check) reloadComment(value.bug_id);
              else reloadChildComment(value.parent_id);
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
const checkEditComment = ref(false);
const checkEditCommentChild = ref(false);
const bugComment = ref();
const editComment = (value, check) => {
  bugComment.value = value;
  if (check) checkEditComment.value = value.comment_id;
  else checkEditCommentChild.value = value.comment_id;
};
const delEditFileComment = (url) => {
  bugComment.value.url_file = bugComment.value.url_file.filter((x) => x != url);
};
const saveEditComment = (item, check) => {
  let url_files = "";
  let detached = "";
  if (Array.isArray(bugComment.value.url_file))
    bugComment.value.url_file.forEach((element) => {
      url_files += detached + "/Portals/CommentBug/" + element.data;
      detached = ",";
    });
  bugComment.value.url_file = url_files;
  checkEditComment.value = null;
  checkEditCommentChild.value = null;
  axios
    .put(
      baseURL + "/api/api_commentbug/Update_commentbug",
      bugComment.value,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa bình luận thành công!");
        isDetailsBug.value = true;
        if (check) reloadComment(Bug.value.bug_id);
        else reloadChildComment(bugComment.value.parent_id);
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
      console.log(error);
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const checkAddChildCmt = ref();
const showAddChildCmt = (value) => {
  commentChild.value = "";
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_list",
          par: [{ par: "bug_id", va: Bug.value.bug_id },  { par: "commentbug_id",va:null }],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );
        });
        listCommentBugSave.value = data;
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_listwithid",
          par: [
            { par: "bug_id", va: value.bug_id },
            { par: "parent_id", va: value.comment_id },   { par: "bugcomment_id", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );

          listCommentBugSave.value.push(element);
        });
        renderComment(listCommentBugSave.value);

        checkAddChildCmt.value = value.comment_id;
      })
      .catch((error) => {
        console.log(error);
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};

const renderComment = (listComment) => {
  let arrChils = [];
  listComment
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = { key: m.comment_id, data: m };
      const rechildren = (mm, comment_id) => {
        if (!mm.children) mm.children = [];
        let dts = listComment.filter((x) => x.parent_id == comment_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = { key: em.comment_id, data: em };
            rechildren(om1, em.comment_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.comment_id);
      arrChils.push(om);
    });
  listCommentBugs.value = arrChils;
};
const fileDataCode = ref();
const checkFileCode = ref(false);
const showFileCode = (value) => {
  checkFileCode.value = true;
  axios
    .get(baseURL + "/api/api_category/GetFileCode?filePath=" + value.url_file, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.data)
        fileDataCode.value = response.data.data.join("\n");
      else fileDataCode.value = "Không có dữ liệu!";
    })
    .catch((error) => {
      console.log("ree", error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        //  store.commit("gologout");
      }
    });
};
const isShowAddBug = ref(false);
const headerAddBug = ref("");
const addBug = () => {
  submitted.value = false;
  headerAddBug.value = "Thêm lỗi";
  isSaveBug.value = false;
  bug.value = {
    project_id: projectSelected.value,
    service_id: serviceId.value,
    bug_name: "",
    des: "",
    status: 0,
    keyword: "",
  };
  isShowAddBug.value = true;
};
const editBug = (value) => {
  isSaveBug.value = true;
  if (value.keyword != null && value.keyword.length > 1) {
    if (!Array.isArray(value.keyword)) {
      value.keyword = value.keyword.split(",");
    }
  }
  bug.value = value;
  submitted.value = false;
  headerAddBug.value = "Sửa lỗi";
  isShowAddBug.value = true;
};
const deleteBug = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa lỗi này không!",
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
          .delete(baseURL + "/api/api_bug/Delete_bug", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.bug_id != null ? [value.bug_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá lỗi thành công!");
              showBugs(serviceId.value);
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
const commentChild = ref();
const isShowEmoji = ref(false);
const showEmoji = () => {
  if (isShowEmoji.value) isShowEmoji.value = false;
  else isShowEmoji.value = true;
};
const handleEmojiClick = (event) => {
  if (comment.value) comment.value = comment.value + event.unicode;
  else comment.value = event.unicode;
};

const chonanh = (id) => {
  document.getElementById(id).click();
};
const checkFileComment = ref(false);
const listFileComment = ref([]);
const handleFileUpload = (event) => {
  listFileComment.value = [];
  filecoments = [];
  filecoments = event.target.files;
  if (filecoments) {
    checkFileComment.value = true;
    for (let index = 0; index < filecoments.length; index++) {
      const element = filecoments[index];

      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileComment.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        URL.revokeObjectURL(element);
      } else {
        listFileComment.value.push({
          data: element,
          src: element.name,
          checkimg: false,
        });
      }
    }
  }
};
const delImgComment = (value) => {
  let arrImg = [];
  for (let index = 0; index < filecoments.length; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
};

const isShowEmojiChild = ref(false);
const showEmojiChild = () => {
  if (isShowEmojiChild.value) isShowEmojiChild.value = false;
  else isShowEmojiChild.value = true;
};
const handleEmojiClickChild = (event) => {
  commentChild.value = commentChild.value + event.unicode;
};

const checkFileCommentChild = ref(false);
const listFileCommentChild = ref([]);
const handleFileUploadChild = (event) => {
  listFileCommentChild.value = [];
  filecoments = [];
  filecoments = event.target.files;
  if (filecoments) {
    checkFileCommentChild.value = true;
    for (let index = 0; index < filecoments.length; index++) {
      const element = filecoments[index];

      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileCommentChild.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        URL.revokeObjectURL(element);
      } else {
        listFileCommentChild.value.push({
          data: element,
          src: element.name,
          checkimg: false,
        });
      }
    }
  }
};
const delImgCommentChild = (value) => {
  let arrImg = [];
  for (let index = 0; index < filecoments.length; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileCommentChild.value = listFileCommentChild.value.filter(
    (x) => x.data != value
  );
};
const onHideEmoji = () => {
  isShowEmoji.value = false;
};
const onHideEmojiChild = () => {
  isShowEmojiChild.value = false;
};
//Thêm bản ghi
let filecoments = [];
onMounted(() => {
  loadProject();
  return {
    loadProject,
    headerAPI,
  };
});
</script>
<template>
  <div class="surface-100 w-full">
    <div class="w-full">
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

          <div style="height: calc(100vh - 130px)">
            <TreeTable
              :value="listCategory"
              :filters="filters"
              @nodeSelect="onNodeSelect"
              @node-unselect="onUnNodeSelect"
              selectionMode="single"
              v-model:selectionKeys="selectedKey"
              class="h-full w-full overflow-x-hidden p-0"
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
                          @click="openBasic('Thêm loại API')"
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
                    v-if="!data.node.data.service_id"
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
                          <div class="px-2" style="line-height: 36px">
                            {{ data.node.data.category_name }}
                            <span v-if="data.node.data.category_name">
                              <small v-for="item in Listcount" :key="item.key">
                                <span
                                  v-if="item.key == data.node.data.category_id"
                                  >({{ item.count }})</span
                                >
                              </small>
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="relative flex w-full p-0" v-else>
                    <div
                      v-if="!data.node.data.parameters_id"
                      class="flex w-full p-0"
                    >
                      <div class="grid w-full p-0">
                        <div
                          class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2 format-center"
                        >
                          <div class="col-2 p-0">
                            <img
                              src="../../assets/image/service.png"
                              class="pr-2 pb-0"
                              width="28"
                              height="36"
                              style="object-fit: contain"
                            />
                          </div>
                          <div class="col-9 p-0">
                            <div style="line-height: 36px">
                              {{ data.node.data.service_name }}
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div v-else class="flex w-full p-0">
                      <div class="grid w-full p-0">
                        <div
                          class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                        >
                          <div class="col-2 p-0">
                            <img
                              src="../../assets/image/paramester.png"
                              class="pr-2 pb-2"
                              width="28"
                              height="36"
                              style="object-fit: contain"
                            />
                          </div>
                          <div class="col-10 p-0">
                            <div style="line-height: 36px">
                              {{ data.node.data.parameters_name }}
                            </div>
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
          <div>
            <div class="d-lang-table mx-3">
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
              >
                <template #header>
                  <div>
                    <Toolbar class="w-full custoolbar p-0">
                      <template #start>
                        <h3 class="m-0">
                          <i class="pi pi-sitemap py-3"></i> Danh sách API
                          <span v-if="options.totalRecords > 0"
                            >({{ options.totalRecords }})</span
                          >
                        </h3>
                      </template>
                      <template #end>
                        <div>
                          <Button
                            v-if="checkParent"
                            @click="preFolder"
                            icon="pi
pi-arrow-left"
                          ></Button>
                        </div>
                      </template>
                    </Toolbar>
                    <Toolbar class="w-full custoolbar">
                      <template #start>
                        <span class="p-input-icon-left">
                          <i class="pi pi-search" />
                          <InputText
                            type="text"
                            class="p-inputtext-sm"
                            spellcheck="false"
                            placeholder="Tìm kiếm"
                            v-model="keySearch"
                            @keypress.enter="searchAPI()"
                          />
                        </span>
                      </template>

                      <template #end>
                        <Button
                          @click="showSearchKey"
                          icon="pi pi-bookmark"
                          class="p-button-rounded p-button-secondary"
                        />
                        <DataViewLayoutOptions v-model="layout" class="mx-2" />

                        <Button
                          v-if="nodeSelected"
                          label="Thêm mới"
                          icon="pi pi-plus"
                          class="p-button-sm mr-2"
                          @click="addService"
                        />
                        <Button
                          class="mr-2 p-button-sm p-button-outlined p-button-secondary"
                          icon="pi pi-refresh"
                          @click="refreshService"
                        />
                        <Button
                          label="Tiện ích"
                          icon="pi pi-file-excel"
                          class="mr-2 p-button-outlined p-button-secondary"
                          aria-haspopup="true"
                          aria-controls="overlay_Export"
                          @click="toggleExport"
                        />
                        <Menu
                          id="overlay_Export"
                          ref="serviceButs"
                          :popup="true"
                          :model="itemButs"
                        />
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
                              class="align-items-center justify-content-center text-center"
                              v-if="slotProps.data.parameters_id"
                            >
                              <Avatar
                                image="./src/assets/image/paramester.png"
                                class="mr-2"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div v-else>
                              <div
                                class="align-items-center justify-content-center text-center"
                                v-if="slotProps.data.project_id"
                              >
                                <Avatar
                                  image="./src/assets/image/folder.png"
                                  class="mr-2"
                                  size="xlarge"
                                  shape="square"
                                />
                              </div>
                              <div
                                v-if="!slotProps.data.project_id"
                                class="align-items-center justify-content-center text-center"
                              >
                                <Avatar
                                  image="./src/assets/image/service.png"
                                  class="mr-2"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
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
                      <template #subtitle>
                        <div
                          @click="showDetails(slotProps)"
                          class="cursor-pointer"
                        >
                          <div v-if="!slotProps.data.project_id">
                            <i
                              v-if="slotProps.data.is_app"
                              class="pi pi-mobile"
                            ></i>
                            <i
                              v-else
                              class="pi pi-desktop"
                              style="color: transparent"
                            ></i>
                          </div>
                          <div v-else>
                            <i
                              class="pi pi-folder-open"
                              style="color: transparent"
                            ></i>
                          </div>
                        </div>
                      </template>
                      <template #content>
                        <div
                          class="text-center cursor-pointer"
                          @click="showDetails(slotProps)"
                        >
                          <div v-if="slotProps.data.parameters_id">
                            <div
                              class="text-lg text-blue-400 font-bold pb-2"
                              style="word-break: break-all"
                            >
                              {{ slotProps.data.parameters_name }}
                            </div>
                            <div v-html="slotProps.data.des"></div>
                          </div>
                          <div v-else>
                            <div
                              v-if="slotProps.data.project_id"
                              class="mb-1 text-lg text-blue-400 font-bold"
                              style="word-break: break-all"
                            >
                              {{ slotProps.data.category_name }}
                            </div>
                            <div
                              v-else
                              class="mb-1 text-lg text-blue-400 font-bold"
                              style="word-break: break-all"
                            >
                              {{ slotProps.data.service_name }}
                            </div>
                          </div>
                        </div>
                      </template>
                    </Card>
                  </div>
                </template>
                <template #list="slotProps">
                  <div class="w-full">
                    <div class="flex align-items-center justify-content-center">
                      <div
                        class="flex flex-column flex-grow-1 surface-0 m-2 border-round-xs pl-3 pt-3"
                        :class="'row ' + slotProps.data.active"
                  
                      >
                        <div class="col-12 field flex p-0 m-0 px-2">
                          <div
                            class="col-8 p-0 cursor-pointer"
                            @click="showDetails(slotProps)"
                          >
                            <div class="col-12 p-0 font-bold text-xl">
                              <div v-if="slotProps.data.parameters_id">
                                {{ slotProps.data.parameters_name }}
                              </div>
                              <div v-else>
                                <div
                                  v-if="slotProps.data.project_id"
                                  class="mb-1 font-bold text-xl"
                                >
                                  {{ slotProps.data.category_name }}
                                </div>
                                <div
                                  v-else
                                  class="mb-1 font-italic text-color-secondary"
                                  style="
                                    display: flex;
                                    align-items: center;
                                    vertical-align: middle;
                                    text-align: center;
                                  "
                                >
                                  <Tag
                                    class="method"
                                    type="button"
                                    style="margin-right: 10px"
                                    v-if="slotProps.data.method == 'HttpPost'"
                                    >{{ slotProps.data.method }}</Tag
                                  >
                                  <Tag
                                    class="method"
                                    style="
                                      background-color: #689f38;
                                      margin-right: 10px;
                                    "
                                    type="button"
                                    v-else-if="
                                      slotProps.data.method == 'HttpPut'
                                    "
                                    >{{ slotProps.data.method }}</Tag
                                  >
                                  <Tag
                                    class="method"
                                    style="
                                      background-color: #d32f2f;
                                      margin-right: 10px;
                                    "
                                    type="button"
                                    v-else-if="
                                      slotProps.data.method == 'HttpDelete'
                                    "
                                    >{{ slotProps.data.method }}</Tag
                                  >

                                  <Tag
                                    class="method"
                                    style="
                                      background: #607d8b;
                                      margin-right: 10px;
                                    "
                                    type="button"
                                    v-else-if="
                                      slotProps.data.method == 'HttpGet'
                                    "
                                    >{{ slotProps.data.method }}</Tag
                                  >
                                  <div v-else></div>
                                  {{ slotProps.data.service_name }}
                                  <span
                                    v-if="slotProps.data.parameters.length > 0"
                                  >
                                    (
                                    <small
                                      class="font-normal font-italic"
                                      v-for="(parameter, index) of slotProps
                                        .data.parameters"
                                      :key="index"
                                    >
                                      <span class="text-primary">
                                        {{ parameter.parameters_type }}</span
                                      >
                                      {{ parameter.parameters_name
                                      }}<span
                                        v-if="
                                          index + 1 <
                                          slotProps.data.parameters.length
                                        "
                                        >,
                                      </span>
                                    </small>
                                    )
                                  </span>
                                </div>
                              </div>
                            </div>
                            <div class="col-12 p-0">
                              <div
                                v-if="slotProps.data.parameters_id"
                                v-html="slotProps.data.des"
                              ></div>
                              <div v-else>
                                <div
                                  v-if="slotProps.data.project_id"
                                  class="mb-1 font-italic text-color-secondary"
                                ></div>
                                <div
                                  v-else
                                  class="mb-1 font-italic text-color-secondary"
                                  v-html="slotProps.data.des"
                                ></div>
                              </div>
                            </div>
                          </div>
                          <div class="col-4 text-right flex">
                            <Toolbar
                              class="w-full surface-0 outline-none border-none p-0"
                              :class="'row ' + slotProps.data.active"
                            >
                              <template #start
                                ><Button @click="showFileCode(slotProps.data)"
                                  >File</Button
                                >
                              </template>
                              <template #end>
                                <div class="flex">
                                  <div
                                    class="format-center w-full mr-8"
                                    @click="showBugs(slotProps.data.service_id)"
                                  >
                                    <div v-if="slotProps.data.checkbug == 1">
                                      <img
                                        src="/src/assets/image/bug.png"
                                        alt=""
                                        width="40"
                                        height="40"
                                        class="cursor-pointer"
                                      />
                                    </div>
                                    <div v-else>
                                      <img
                                        v-if="slotProps.data.checkbug == 2"
                                        src="/src/assets/image/closebug.png"
                                        alt=""
                                        width="40"
                                        height="40"
                                        class="cursor-pointer"
                                      />
                                      <img
                                        v-if="slotProps.data.checkbug == 0"
                                        src="/src/assets/image/nobug.png"
                                        alt=""
                                        width="40"
                                        height="40"
                                        class="cursor-pointer"
                                      />
                                    </div>
                                  </div>
                                  <Button
                                    icon="pi pi-ellipsis-h"
                                    class="p-button-outlined p-button-secondary ml-2 border-none"
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
                        <div class="col-12 field flex p-0 m-0 px-2 pb-2">
                          <div
                            class="cursor-pointer"
                            @click="showDetails(slotProps)"
                          >
                            <div v-if="slotProps.data.parameters_id">
                              <img
                                src="../../assets/image/paramester.png"
                                width="28"
                                height="36"
                                style="object-fit: contain"
                              />
                            </div>
                            <div v-else>
                              <div v-if="slotProps.data.project_id">
                                <img
                                  src="../../assets/image/folder.png"
                                  width="28"
                                  height="36"
                                  style="object-fit: contain"
                                />
                              </div>
                              <div v-else>
                                <div v-if="slotProps.data.is_app">
                                  <Button class="p-0" type="button">APP</Button>
                                </div>
                                <div v-else>
                                  <Button
                                    class="border-none"
                                    type="button"
                                    style="
                                      color: transparent;
                                      background-color: transparent;
                                    "
                                    >APP</Button
                                  >
                                </div>
                              </div>
                            </div>
                          </div>
                          <div
                            class="pl-8 pt-2 cursor-pointer"
                            @click="showDetails(slotProps)"
                          >
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
                          <div
                            class="px-8 pt-2 cursor-pointer"
                            @click="showDetails(slotProps)"
                          >
                            <i class="pi pi-tags text-color-secondary"></i>
                            {{ categoryName }}
                          </div>
                          <div
                            class="px-8 pt-2 cursor-pointer"
                            @click="showDetails(slotProps)"
                          >
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
                    class="align-items-center justify-content-center p-4 text-center"
                    v-if="!isFirst"
                  >
                    <img
                      src="../../assets/background/nodata.png"
                      style="height: 144px"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataView>
              <div>
                <Sidebar
                  v-model:visible="isShowBug"
                  :baseZIndex="100"
                  position="right"
                  class="p-sidebar-lg py-0 overflow-hidden"
                >
                  <div v-if="!isDetailsBug">
                    <h2>{{ serviceName }}</h2>
                  </div>
                  <div v-if="!isDetailsBug">
                    <DataView
                      class="w-full h-full e-sm flex flex-column"
                      responsiveLayout="scroll"
                      :scrollable="true"
                      layout="list"
                      :lazy="true"
                      :value="listBugSave"
                    >
                      <template #header>
                        <div>
                          <Toolbar class="w-full custoolbar p-0">
                            <template #start>
                              <h3 class="m-0 flex">
                                <img
                                  src="/src/assets/image/iconbug.png"
                                  alt=""
                                  width="20"
                                  height="20"
                                  class="cursor-pointer"
                                />
                                <span class="ml-1">Danh sách Bug</span>
                                <span v-if="listBugSave.length > 0"
                                  >({{ listBugSave.length }})</span
                                >
                              </h3>
                            </template>
                            <template #end> </template>
                          </Toolbar>
                          <Toolbar class="w-full custoolbar pt-5">
                            <template #start>
                              <span class="p-input-icon-left">
                                <i class="pi pi-search" />
                                <InputText
                                  type="text"
                                  class="p-inputtext-sm"
                                  spellcheck="false"
                                  placeholder="Tìm kiếm"
                                  v-model="options.searchTextBug"
                                  @keyup.enter="showBugs(serviceId)"
                                />
                              </span>
                            </template>

                            <template #end>
                              <!-- <DataViewLayoutOptions v-model="layout" class="mx-2" /> -->

                              <Button
                                label="Thêm mới"
                                icon="pi pi-plus"
                                class="p-button-sm mr-2"
                                @click="addBug"
                              />
                              <Button
                                class="mr-2 p-button-sm p-button-outlined p-button-secondary"
                                icon="pi pi-refresh"
                              />
                              <Button
                                label="Tiện ích"
                                icon="pi pi-file-excel"
                                class="mr-2 p-button-outlined p-button-secondary"
                                aria-haspopup="true"
                                aria-controls="overlay_Export"
                              />
                              <Menu
                                id="overlay_Export"
                                ref="serviceButs"
                                :popup="true"
                              />
                            </template>
                          </Toolbar>
                        </div>
                      </template>

                      <template #list="slotProps">
                        <div class="w-full">
                          <div
                            class="flex align-items-center justify-content-center"
                          >
                            <div
                              class="flex flex-column flex-grow-1 surface-0 m-2 border-round-xs pl-3 pt-3"
                            >
                              <div class="col-12 field flex p-0 m-0">
                                <div
                                  @click="showCommentBug(slotProps.data)"
                                  class="col-10 p-0 cursor-pointer"
                                >
                                  <div
                                    class="col-12 p-0 font-bold text-xl flex"
                                    style="font-size: 1rem"
                                  >
                                    <div class="mb-1 font-bold text-xl pt-2">
                                      <Tag
                                        icon="pi pi-hashtag"
                                        style="
                                          background-color: black;
                                          color: white;
                                        "
                                      >
                                        {{ slotProps.data.bug_id }}
                                      </Tag>
                                    </div>
                                    <div
                                      class="mb-1 font-bold text-xl px-2 pt-2"
                                    >
                                      {{ slotProps.data.bug_name }}
                                    </div>
                                    <div
                                      v-if="slotProps.data.status"
                                      class="mb-1 font-bold text-xl px-1 pt-2"
                                    >
                                      <div
                                        v-if="slotProps.data.status == 1"
                                        class="mb-1 font-italic text-color-secondary"
                                      >
                                        <Tag severity="success">Đã sửa</Tag>
                                      </div>
                                      <div
                                        v-else-if="slotProps.data.status == -1"
                                        class="mb-1 font-italic text-color-secondary"
                                      >
                                        <Tag severity="info">Đang sửa</Tag>
                                      </div>
                                      <div>
                                        <div
                                          class="mb-1 font-italic text-color-secondary"
                                        >
                                          <Tag
                                            v-if="slotProps.data.status == -2"
                                            severity="danger"
                                            >Lỗi</Tag
                                          >
                                        </div>
                                      </div>
                                      <div>
                                        <div
                                          class="mb-1 font-italic text-color-secondary"
                                        >
                                          <Tag
                                            v-if="slotProps.data.status == 2"
                                            severity="warning"
                                            >Đã đóng</Tag
                                          >
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                                <div class="col-2 text-right flex">
                                  <Toolbar
                                    class="w-full surface-0 outline-none border-none p-0"
                                  >
                                    <template #start> </template>
                                    <template #end>
                                      <div>
                                        <Button
                                          icon="pi pi-ellipsis-h"
                                          class="p-button-outlined p-button-secondary ml-2 border-none"
                                          @click="
                                            toggleBugsMores(
                                              $event,
                                              slotProps.data
                                            )
                                          "
                                          aria-haspopup="true"
                                          aria-controls="overlay_BugsMore"
                                        />
                                        <Menu
                                          id="overlay_BugsMore"
                                          ref="menuButBugsMores"
                                          :model="itemButBugsMores"
                                          :popup="true"
                                        />
                                      </div>
                                    </template>
                                  </Toolbar>
                                </div>
                              </div>
                              <div
                                @click="showCommentBug(slotProps.data)"
                                class="col-12 field flex p-0 m-0 px-2 pb-2 cursor-pointer"
                              >
                                <div class="pl-0 pt-0">
                                  <div>
                                    Mở
                                    <timeago
                                      :datetime="slotProps.data.created_date"
                                      :locale="vi"
                                    />
                                  </div>
                                </div>
                                <div class="pl-1 pt-0">
                                  <div>
                                    bởi
                                    <span class="text-primary">
                                      {{ slotProps.data.created_by }}</span
                                    >
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </template>
                      <template #empty>
                        <div
                          class="align-items-center justify-content-center p-4 text-center"
                          v-if="!isFirst"
                        >
                          <img
                            src="../../assets/background/nodata.png"
                            style="height: 144px"
                          />
                          <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                      </template>
                    </DataView>
                  </div>
                  <div v-else class="relative comment-height">
                    <div class="fixed top-0 flex">
                      <div class="flex format-center">
                        <Button
                          icon="pi
pi-arrow-left"
                          class="h-2rem pt-2"
                          @click="preListBug()"
                        ></Button>
                        <h2 class="ml-2">{{ Bug.bug_name }}</h2>
                      </div>
                    </div>

                    <div>
                      Nguời tạo:
                      <span class="font-semibold pr-1"
                        >{{ Bug.created_by }}
                      </span>

                      <timeago :datetime="Bug.created_date" :locale="vi" />
                    </div>

                    <hr />
                    <div class="pl-2" v-html="Bug.des"></div>
                    <div class="col-10 p-0" v-if="Bug.url_file">
                      <div class="flex">
                        <Toolbar class="w-full py-3">
                          <template #start>
                            <div class="flex">
                              <img
                                src="/src/assets/image/filess.png"
                                style="object-fit: contain"
                                width="50"
                                height="50"
                                alt="logorar"
                              />
                              <span style="line-height: 50px">
                                {{ Bug.url_file.substring(16) }}</span
                              >
                            </div>
                          </template>
                          <!-- <template #end>
                            <Button
                              icon="pi pi-times"
                              class="p-button-rounded p-button-danger"
                              @click="deleteFileBug()"
                            />
                          </template> -->
                        </Toolbar>
                      </div>
                    </div>
                    <div v-for="(item, idxItem) in Bug.keyword" :key="idxItem" class="m-1">
                      <Chip>
                        {{ item }}
                      </Chip>
                    </div>

                    <div class="pt-3">
                      <div class="px-3 pb-3">Bình luận:</div>

                      <div class="flex px-3">
                        <div class="grid w-full">
                          <div class="col-12 p-0 flex">
                            <div
                              class="col-10 p-0 ml-2 border-1 border-round-xs border-600 flex"
                              style="border-radius: 5px"
                            >
                              <Textarea
                                style="border-radius: 5px"
                                class="border-0 col-10 pb-0"
                                placeholder="Viết bình luận..."
                                :autoResize="true"
                                rows="1"
                                v-model="comment"
                              />
                              <div class="col-2 p-0 relative">
                                <div class="absolute bottom-0 flex">
                                  <Button
                                    v-clickoutside="onHideEmoji"
                                    class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                                    @click="showEmoji"
                                  >
                                    <img
                                      alt="logo"
                                      src="/src/assets/image/smile.png"
                                      width="20"
                                      height="20"
                                    />
                                  </Button>
                                  <div
                                    v-if="isShowEmoji"
                                    class="absolute right-0 top-100"
                                  >
                                    <VuemojiPicker
                                      @emojiClick="handleEmojiClick"
                                    />
                                  </div>
                                  <Button
                                    class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                                    @click="chonanh('AnhTem')"
                                  >
                                    <img
                                      alt="logo1"
                                      src="/src/assets/image/imageicon.png"
                                      width="20"
                                      height="20"
                                    />
                                  </Button>
                                  <Button
                                    @click="chonanh('FileComment')"
                                    class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                                  >
                                    <img
                                      alt="logo2"
                                      src="/src/assets/image/filesymbol.png"
                                      width="20"
                                      height="20"
                                    />
                                  </Button>
                                  <input
                                    class="hidden"
                                    id="AnhTem"
                                    type="file"
                                    multiple="true"
                                    accept="image/*"
                                    @change="handleFileUpload"
                                  />
                                  <input
                                    class="hidden"
                                    id="FileComment"
                                    type="file"
                                    multiple="true"
                                    @change="handleFileUpload"
                                  />
                                  <!-- Kiểm tra xem file là ảnh hoặc File rồi ấy ấy -->
                                </div>
                              </div>
                            </div>
                            <div
                              class="w-2 h-full px-3 format-center col-2 p-0"
                            >
                              <Button
                                icon="pi pi-send"
                                @click="addComment(true, null)"
                                class="w-full h-full ml-3"
                              />
                            </div>
                          </div>
                          <div class="col-12 flex" v-if="checkFileComment">
                            <div
                              v-for="(item, index) in listFileComment"
                              :key="index"
                              class="mr-2 relative"
                            >
                              <Button
                                @click="delImgComment(item.data)"
                                icon="pi pi-times"
                                class="p-button-rounded p-button-text p-button-plain absolute top-0 right-0 w-1rem h-1rem"
                              ></Button>
                              <img
                                v-if="item.checkimg"
                                :src="item.src"
                                :alt="item.data.name"
                                style="
                                  width: 100px;
                                  height: 100px;
                                  object-fit: contain;
                                "
                              />
                              <div v-else>
                                <img
                                  :src="
                                    '/src/assets/image/file/' +
                                    item.data.name.substring(
                                      item.data.name.indexOf('.') + 1
                                    ) +
                                    '.png'
                                  "
                                  style="
                                    width: 50px;
                                    height: 50px;
                                    object-fit: contain;
                                  "
                                  :alt="item.data.name"
                                />
                                <div>{{ item.data.name }}</div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div
                      class="overflow-y-auto overflow-x-hidden comment-height-scroll"
                    >
                      <div
                        v-for="(item, index) in listCommentBugs"
                        :key="index"
                      >
                        <!-- v-if="item.user_id != store.getters.user.Users_ID" -->
                        <div class="w-full" :id="item.data.comment_id">
                          <div class="grid formgrid m-2">
                            <div class="field col-12 md:col-12 flex p-0">
                              <div class="col-1 md:col-1 pl-2 pr-3">
                                <Avatar
                                  :image="basedomainURL + item.data.avatar"
                                  class="w-full"
                                  size="large"
                                  shape="circle"
                                />
                              </div>
                              <div class="col-11 md:col-11 p-0">
                                <Panel class="w-full">
                                  <template #header>
                                    <div class="flex relative w-full">
                                      <span class="font-bold pr-1"
                                        >{{ item.data.created_by }}
                                      </span>
                                      <timeago
                                        :datetime="item.data.created_date"
                                        :locale="vi"
                                      />
                                      <div
                                        v-if="
                                          item.data.user_id ==
                                          store.getters.user.Users_ID
                                        "
                                        class="absolute right-0 bottom-0"
                                      >
                                        <Button
                                          class="p-button-rounded p-button-secondary p-button-text"
                                          @click="editComment(item.data, true)"
                                          type="button"
                                          icon="pi pi-pencil"
                                        ></Button>
                                        <Button
                                          class="p-button-rounded p-button-secondary p-button-text"
                                          @click="
                                            deleteComment(item.data, true)
                                          "
                                          type="button"
                                          icon="pi pi-trash"
                                        ></Button>
                                      </div>
                                    </div>
                                  </template>

                                  <div
                                    v-if="
                                      item.data.comment_id != checkEditComment
                                    "
                                  >
                                    <span v-html="item.data.des"></span>
                                    <div>
                                      <div
                                        v-for="(element, index) in item.data
                                          .url_file"
                                        :key="index"
                                        class="mr-2 relative"
                                      >
                                        <img
                                          v-if="element.checkimg"
                                          :src="element.src"
                                          :alt="element.data"
                                          style="
                                            width: 100px;
                                            height: 100px;
                                            object-fit: contain;
                                          "
                                        />
                                        <div v-else>
                                          <a
                                            :href="element.src"
                                            download
                                            class="w-full no-underline"
                                          >
                                            <img
                                              :src="
                                                '/src/assets/image/file/' +
                                                element.data.substring(
                                                  element.data.indexOf('.') + 1
                                                ) +
                                                '.png'
                                              "
                                              style="
                                                width: 50px;
                                                height: 50px;
                                                object-fit: contain;
                                              "
                                              :alt="element.data"
                                            />
                                            <div>{{ element.data }}</div></a
                                          >
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                  <div v-else>
                                    <div class="w-full h-full p-0 m-0 flex">
                                      <Textarea
                                        style="border-radius: 5px"
                                        class="w-full"
                                        :autoResize="true"
                                        rows="1"
                                        v-model="bugComment.des"
                                      />

                                      <Button
                                        icon="pi pi-check"
                                        @click="saveEditComment(item, true)"
                                        class="w-2"
                                      />
                                    </div>
                                    <div>
                                      <div
                                        v-for="(
                                          element, stt
                                        ) in bugComment.url_file"
                                        :key="stt"
                                        class="mr-2 relative my-2"
                                      >
                                        <Button
                                          icon="pi pi-times"
                                          class="p-button-rounded p-button-text p-button-plain absolute top-0 right-0 w-1rem h-1rem"
                                          @click="
                                            delEditFileComment(element, item)
                                          "
                                        ></Button>
                                        <img
                                          v-if="element.checkimg"
                                          :src="element.src"
                                          :alt="element.data"
                                          style="
                                            width: 100px;
                                            height: 100px;
                                            object-fit: contain;
                                          "
                                        />
                                        <div v-else>
                                          <a
                                            :href="element.src"
                                            download
                                            class="w-full no-underline"
                                          >
                                            <img
                                              :src="
                                                '/src/assets/image/file/' +
                                                element.data.substring(
                                                  element.data.indexOf('.') + 1
                                                ) +
                                                '.png'
                                              "
                                              style="
                                                width: 50px;
                                                height: 50px;
                                                object-fit: contain;
                                              "
                                              :alt="element.data"
                                            />
                                            <div>{{ element.data }}</div></a
                                          >
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </Panel>
                                <div
                                  v-if="
                                    checkAddChildCmt != item.data.comment_id
                                  "
                                  @click="showAddChildCmt(item.data)"
                                  class="cursor-pointer font-bold"
                                >
                                  Phản hồi
                                </div>
                                <div
                                  @click="showAddChildCmt(item.data)"
                                  class="flex cursor-pointer ml-3"
                                  v-if="
                                    checkAddChildCmt != item.data.comment_id
                                  "
                                >
                                  <div
                                    v-if="item.data.haschild > 0"
                                    class="flex"
                                  >
                                    <Button
                                      class="p-button-rounded p-button-secondary p-button-text"
                                      type="button"
                                      icon="pi pi-reply"
                                    ></Button>
                                    <div style="line-height: 36px">
                                      Xem thêm
                                    </div>
                                  </div>
                                </div>
                                <div v-else class="flex m-3">
                                  <div class="grid w-full">
                                    <div
                                      class="field col-12 md:col-12 p-0"
                                      v-if="item.children.length > 0"
                                    >
                                      <div
                                        v-for="element in item.children"
                                        :key="element.key"
                                        class="col-12 m-0 p-0 flex py-1"
                                      >
                                        <div class="col-1 md:col-1 pl-2 pr-3">
                                          <Avatar
                                            :image="
                                              basedomainURL +
                                              element.data.avatar
                                            "
                                            class="w-2rem h-2rem m-2"
                                            size="large"
                                            shape="circle"
                                          />
                                        </div>
                                        <div class="col-11 md:col-11 p-0">
                                          <Panel class="w-full">
                                            <template #header>
                                              <div
                                                class="flex relative w-full p-1"
                                              >
                                                <span class="font-bold pr-1"
                                                  >{{ element.data.created_by }}
                                                </span>
                                                <timeago
                                                  :datetime="
                                                    element.data.created_date
                                                  "
                                                  :locale="vi"
                                                />
                                                <div
                                                  v-if="
                                                    element.data.user_id ==
                                                    store.getters.user.Users_ID
                                                  "
                                                  class="absolute right-0 bottom-0"
                                                >
                                                  <Button
                                                    class="p-button-rounded p-button-secondary p-button-text"
                                                    @click="
                                                      editComment(
                                                        element.data,
                                                        false
                                                      )
                                                    "
                                                    type="button"
                                                    icon="pi pi-pencil"
                                                  ></Button>
                                                  <Button
                                                    class="p-button-rounded p-button-secondary p-button-text"
                                                    @click="
                                                      deleteComment(
                                                        element.data,
                                                        false
                                                      )
                                                    "
                                                    type="button"
                                                    icon="pi pi-trash"
                                                  ></Button>
                                                </div>
                                              </div>
                                            </template>
                                            <div
                                              v-if="
                                                element.data.comment_id !=
                                                checkEditCommentChild
                                              "
                                            >
                                              <span
                                                v-html="element.data.des"
                                              ></span>
                                              <div>
                                                <div
                                                  v-for="(
                                                    element1, index1
                                                  ) in element.data.url_file"
                                                  :key="index1"
                                                  class="mr-2 relative"
                                                >
                                                  <img
                                                    v-if="element1.checkimg"
                                                    :src="element1.src"
                                                    :alt="element1.data"
                                                    style="
                                                      width: 100px;
                                                      height: 100px;
                                                      object-fit: contain;
                                                    "
                                                  />
                                                  <div v-else>
                                                    <a
                                                      :href="element1.src"
                                                      download
                                                      class="w-full no-underline"
                                                    >
                                                      <img
                                                        :src="
                                                          '/src/assets/image/file/' +
                                                          element1.data.substring(
                                                            element1.data.indexOf(
                                                              '.'
                                                            ) + 1
                                                          ) +
                                                          '.png'
                                                        "
                                                        style="
                                                          width: 50px;
                                                          height: 50px;
                                                          object-fit: contain;
                                                        "
                                                        :alt="element1.data"
                                                      />
                                                      <div>
                                                        {{ element1.data }}
                                                      </div></a
                                                    >
                                                  </div>
                                                </div>
                                              </div>
                                            </div>
                                            <div v-else>
                                              <div
                                                class="w-full h-full p-0 m-0 flex"
                                              >
                                                <Textarea
                                                  style="border-radius: 5px"
                                                  class="w-full"
                                                  :autoResize="true"
                                                  rows="1"
                                                  v-model="bugComment.des"
                                                />

                                                <Button
                                                  icon="pi pi-check"
                                                  @click="
                                                    saveEditComment(
                                                      item.data,
                                                      false
                                                    )
                                                  "
                                                  class="w-2"
                                                />
                                              </div>
                                              <div>
                                                <div
                                                  v-for="(
                                                    element, stt
                                                  ) in bugComment.url_file"
                                                  :key="stt"
                                                  class="mr-2 relative my-2"
                                                >
                                                  <Button
                                                    icon="pi pi-times"
                                                    class="p-button-rounded p-button-text p-button-plain absolute top-0 right-0 w-1rem h-1rem"
                                                    @click="
                                                      delEditFileComment(
                                                        element,
                                                        item.data
                                                      )
                                                    "
                                                  ></Button>
                                                  <img
                                                    v-if="element.checkimg"
                                                    :src="element.src"
                                                    :alt="element.data"
                                                    style="
                                                      width: 100px;
                                                      height: 100px;
                                                      object-fit: contain;
                                                    "
                                                  />
                                                  <div v-else>
                                                    <a
                                                      :href="element.src"
                                                      download
                                                      class="w-full no-underline"
                                                    >
                                                      <img
                                                        :src="
                                                          '/src/assets/image/file/' +
                                                          element.data.substring(
                                                            element.data.indexOf(
                                                              '.'
                                                            ) + 1
                                                          ) +
                                                          '.png'
                                                        "
                                                        style="
                                                          width: 50px;
                                                          height: 50px;
                                                          object-fit: contain;
                                                        "
                                                        :alt="element.data"
                                                      />
                                                      <div>
                                                        {{ element.data }}
                                                      </div></a
                                                    >
                                                  </div>
                                                </div>
                                              </div>
                                            </div>
                                          </Panel>
                                        </div>
                                      </div>
                                    </div>

                                    <div class="col-12 p-0 flex">
                                      <div
                                        class="pt-2 col-10 p-0 ml-2 border-1 border-round-xs border-600 flex"
                                        style="border-radius: 5px"
                                      >
                                        <Textarea
                                          style="border-radius: 5px"
                                          class="border-0 col-10 pb-0"
                                          placeholder="Viết bình luận..."
                                          autofocus
                                          :autoResize="true"
                                          rows="1"
                                          v-model="commentChild"
                                        />
                                        <div class="col-2 p-0 relative">
                                          <div class="absolute bottom-0 flex">
                                            <Button
                                              v-clickoutside="onHideEmojiChild"
                                              class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                                              @click="showEmojiChild"
                                            >
                                              <img
                                                alt="logo"
                                                src="/src/assets/image/smile.png"
                                                width="20"
                                                height="20"
                                              />
                                            </Button>
                                            <div
                                              v-if="isShowEmojiChild"
                                              class="absolute right-0 top-100"
                                            >
                                              <VuemojiPicker
                                                @emojiClick="
                                                  handleEmojiClickChild
                                                "
                                              />
                                            </div>
                                            <Button
                                              class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                                              @click="chonanh('ImageChild')"
                                            >
                                              <img
                                                alt="logo1"
                                                src="/src/assets/image/imageicon.png"
                                                width="20"
                                                height="20"
                                              />
                                            </Button>
                                            <Button
                                              @click="
                                                chonanh('FileCommentChild')
                                              "
                                              class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                                            >
                                              <img
                                                alt="logo2"
                                                src="/src/assets/image/filesymbol.png"
                                                width="20"
                                                height="20"
                                              />
                                            </Button>
                                            <input
                                              class="hidden"
                                              id="ImageChild"
                                              type="file"
                                              multiple="true"
                                              accept="image/*"
                                              @change="handleFileUploadChild"
                                            />
                                            <input
                                              class="hidden"
                                              id="FileCommentChild"
                                              type="file"
                                              multiple="true"
                                              @change="handleFileUploadChild"
                                            />
                                            <!-- Kiểm tra xem file là ảnh hoặc File rồi ấy ấy -->
                                          </div>
                                        </div>
                                      </div>
                                      <div
                                        class="w-2 h-full px-3 format-center col-2 p-0"
                                      >
                                        <Button
                                          icon="pi pi-send"
                                          @click="
                                            addComment(
                                              false,
                                              item.data.comment_id
                                            )
                                          "
                                          class="w-full h-full ml-3"
                                        />
                                      </div>
                                    </div>
                                    <div
                                      class="col-12 flex"
                                      v-if="checkFileCommentChild"
                                    >
                                      <div
                                        v-for="(item, index) in listFileComment"
                                        :key="index"
                                        class="mr-2 relative"
                                      >
                                        <Button
                                          @click="delImgCommentChild(item.data)"
                                          icon="pi pi-times"
                                          class="p-button-rounded p-button-text p-button-plain absolute top-0 right-0 w-1rem h-1rem"
                                        ></Button>
                                        <img
                                          v-if="item.checkimg"
                                          :src="item.src"
                                          :alt="item.data.name"
                                          style="
                                            width: 100px;
                                            height: 100px;
                                            object-fit: contain;
                                          "
                                        />
                                        <div v-else>
                                          <img
                                            :src="
                                              '/src/assets/image/file/' +
                                              item.data.name.substring(
                                                item.data.name.indexOf('.') + 1
                                              ) +
                                              '.png'
                                            "
                                            style="
                                              width: 50px;
                                              height: 50px;
                                              object-fit: contain;
                                            "
                                            :alt="item.data.name"
                                          />
                                          <div>{{ item.data.name }}</div>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </Sidebar>
              </div>
              <div>
                <Sidebar
                  v-model:visible="checkFileCode"
                  :baseZIndex="100"
                  position="right"
                  class="p-sidebar-lg"
                >
                  <div >
                    <h2>File Code</h2>
                  </div>
                  <div class="flex">
                    <!-- <div v-html="dataCode" class="font-bold"></div> -->
                    <div class="p-1 surface-600">
                      <div
                        class="w-full surface-800 text-0 py-1 h-full"
                        style="color: white !important; word-break: break-all"
                      >
                        <pre class="p-0 m-0">
                          {{ fileDataCode }}
                      </pre
                        >
                      </div>
                    </div>
                  </div>
                </Sidebar>
                <Sidebar
                  v-model:visible="isShowSearhKey"
                  :baseZIndex="100"
                  position="right"
                  class="p-sidebar-lg"
                >
                  <div >
                    <h2>Chọn từ khóa</h2>
                  </div>
                  <div class="flex">
                    <div v-for="(item, idxItem) in listKeyWords" :key="idxItem" class="mr-1">
                      <Chip
                        @click="searchKey(item)"
                        class="py-2 cursor-pointer"
                        :class="'row ' + item.active"
                      >
                        {{ item.key }}
                      </Chip>
                    </div>
                  </div>
                  <div>
                    <DataView
                      class="w-full h-full e-sm flex flex-column"
                      responsiveLayout="scroll"
                      :scrollable="true"
                      layout="list"
                      :lazy="true"
                      :value="listDataKey"
                      :loading="options.loading"
                    >
                      <template #list="slotProps">
                        <div class="w-full">
                          <div
                            class="flex align-items-center justify-content-center"
                          >
                            <div
                              class="flex flex-column flex-grow-1 surface-0 m-2 border-round-xs pl-3 pt-3"
                              :class="'row ' + slotProps.data.active"
                            >
                              <div class="col-12 field flex p-0 m-0 px-2">
                                <div
                                  class="col-8 p-0 cursor-pointer"
                                  @click="showDetailsKey(slotProps)"
                                >
                                  <div class="col-12 p-0 font-bold text-xl">
                                    <div>
                                      <div
                                        class="mb-1 font-italic text-color-secondary"
                                      >
                                        {{ slotProps.data.service_name }}
                                        <!-- <span
                                    v-if="slotProps.data.parameters.length > 0"
                                  >
                                    (
                                    <small
                                      class="font-normal font-italic"
                                      v-for="(parameter, index) of slotProps
                                        .data.parameters"
                                    >
                                      <span class="text-primary">
                                        {{ parameter.parameters_type }}</span
                                      >
                                      {{ parameter.parameters_name
                                      }}<span
                                        v-if="
                                          index + 1 <
                                          slotProps.data.parameters.length
                                        "
                                        >,
                                      </span>
                                    </small>
                                    )
                                  </span> -->
                                      </div>
                                    </div>
                                  </div>
                                  <div class="col-12 p-0">
                                    <div
                                      class="mb-1 font-italic text-color-secondary"
                                      v-html="slotProps.data.des"
                                    ></div>
                                  </div>
                                </div>
                                <div class="col-4 text-right flex">
                                  <Toolbar
                                    class="w-full surface-0 outline-none border-none p-0"
                                    :class="'row ' + slotProps.data.active"
                                  >
                                    <template #start>
                                      <div></div>
                                    </template>
                                    <template #end>
                                      <div>
                                        <Button
                                          icon="pi pi-ellipsis-h"
                                          class="p-button-outlined p-button-secondary ml-2 border-none"
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
                                class="col-12 field flex p-0 m-0 px-2 pb-2 cursor-pointer"
                                @click="showDetailsKey(slotProps)"
                              >
                                <div>
                                  <div v-if="slotProps.data.is_app">
                                    <Button class="p-0" type="button"
                                      >APP</Button
                                    >
                                  </div>
                                  <div v-else>
                                    <Button
                                      class="border-none"
                                      type="button"
                                      style="
                                        color: transparent;
                                        background-color: transparent;
                                      "
                                      >APP</Button
                                    >
                                  </div>
                                </div>
                                <div class="pl-8 pt-2">
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
                                <!-- <div class="px-8 pt-2">
                            <i class="pi pi-tags text-color-secondary"></i>
                            {{ categoryName }}
                          </div> -->
                                <div class="px-8 pt-2">
                                  <div v-if="slotProps.data.created_by">
                                    <i
                                      class="pi pi-user text-color-secondary"
                                    ></i>
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
                          class="align-items-center justify-content-center p-4 text-center"
                          v-if="!isFirst"
                        >
                          <img
                            src="../../assets/background/nodata.png"
                            style="height: 144px"
                          />
                          <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                      </template>
                    </DataView>
                  </div>
                </Sidebar>
                <Sidebar
                  v-model:visible="isCheckParam"
                  :baseZIndex="100"
                  position="right"
                  class="p-sidebar-lg"
                >
                  <div>
                    <h2>{{ headerParam }}</h2>
                  </div>

                  <div>
                    <TreeTable
                      :value="dataListsParam"
                      :scrollable="true"
                      scrollHeight="flex"
                      :lazy="true"
                      :rowHover="true"
                      :showGridlines="true"
                      responsiveLayout="scroll"
                      :loading="options.loading"
                    >
                      <template #header>
                        <div v-if="isCheckParam">
                          <h3 class="m-0">
                            <i class="pi pi-share-alt py-3"></i> Danh sách tham
                            số
                          </h3>
                          <Toolbar class="w-full custoolbar pt-5">
                            <template #start>
                              <span class="p-input-icon-left">
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
                              <Button
                                label="Thêm mới"
                                icon="pi pi-plus"
                                class="mr-2 p-button-sm"
                                @click="addParameter(null)"
                              />
                              <Button
                                class="mr-2 p-button-sm p-button-outlined p-button-secondary"
                                icon="pi pi-refresh"
                                @click="reloadParam(selectedService_Id)"
                              />

                              <Button
                                label="Tiện ích"
                                icon="pi pi-file-excel"
                                class="mr-2 p-button-outlined p-button-secondary"
                                aria-haspopup="true"
                                aria-controls="overlay_Export1"
                                @click="toggleParamExport"
                              />
                              <Menu
                                id="overlay_Export1"
                                ref="paramButs"
                                :popup="true"
                                :model="itemParamButs"
                              />
                            </template>
                          </Toolbar>
                        </div>
                      </template>

                      <Column
                        field="is_order"
                        header="STT"
                        headerStyle="text-align:center;max-width:75px;height:50px"
                        bodyStyle="text-align:center;max-width:75px"
                        class="align-items-center justify-content-center text-center"
                      >
                      </Column>
                      <Column
                        field="parameters_name"
                        header="Tên tham số"
                        :expander="true"
                        headerStyle="height:50px"
                      >
                      </Column>
                      <Column
                        field="parameters_type"
                        header="Kiểu dữ liệu"
                        headerStyle="text-align:center;max-width:150px;height:50px"
                        bodyStyle="text-align:center;max-width:150px;"
                        class="align-items-center justify-content-center text-center"
                      >
                      </Column>

                      <Column

                        field="des"
                        header="Mô tả"
                        headerStyle="max-width:300px;height:50px"
                        bodyStyle="max-width:300px;"
                      >
                        <template #body="data">
                          <div class="pl-2" style="word-break: break-all;" v-html="data.node.data.des"></div>
                        </template>
                      </Column>
                      <Column
                        field="example_value"
                        header="Ví dụ"
                        class="align-items-center justify-content-center text-center"
                        headerStyle="text-align:center;max-width:150px;height:50px"
                        bodyStyle="text-align:center;max-width:150px;"
                      >
                      </Column>

                      <Column
                        header="Chức năng"
                        class="align-items-center justify-content-center text-center"
                        headerStyle="text-align:center;max-width:150px;height:50px"
                        bodyStyle="text-align:center;max-width:150px;"
                      >
                        <template #body="Dispatch">
                          <Button
                            class="p-button-rounded p-button-secondary p-button-outlined mx-1 my-2"
                            v-if="
                              checkCsharpType(
                                Dispatch.node.data.parameters_type
                              )
                            "
                            @click="
                              addParameter(Dispatch.node.data.parameters_id)
                            "
                            type="button"
                            icon="pi pi-plus"
                          ></Button>
                          <Button
                            @click="editParameter(Dispatch.node.data)"
                            class="p-button-rounded p-button-secondary p-button-outlined mx-1 my-2"
                            type="button"
                            icon="pi pi-pencil"
                          ></Button>
                          <Button
                            @click="deleteParameter(Dispatch.node.data)"
                            class="p-button-rounded p-button-secondary p-button-outlined mx-1 my-2"
                            type="button"
                            icon="pi pi-trash"
                          ></Button>
                        </template>
                      </Column>
                      <template #empty>
                        <div
                          class="align-items-center justify-content-center p-4 text-center m-auto"
                          v-if="!isFirst"
                        >
                          <img
                            src="../../assets/background/nodata.png"
                            style="height: 144px"
                          />
                          <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                      </template>
                    </TreeTable>
                  </div>

                  <div v-if="dataService.length > 0">
                    <h3>Dữ liệu trả về</h3>
                    <div v-for="(item, index) in dataService" :key="item.data">
                      <div v-html="item.des" class="font-bold"></div>
                      <div class="p-1 surface-600">
                        <div
                          class="w-full surface-800 text-0 py-1 h-full"
                          style="color: white !important; word-break: break-all"
                          v-html="item.data"
                        ></div>
                      </div>
                      <DataTable
                        :lazy="true"
                        :rowHover="true"
                        :showGridlines="true"
                        responsiveLayout="scroll"
                        :value="keyJsonData[index]"
                        class="w-full"
                      >
                        <Column
                          field="key"
                          header="Key"
                          class="align-items-center justify-content-center text-center"
                          headerStyle="text-align:center;max-width:150px;height:50px"
                          bodyStyle="text-align:center;max-width:150px;"
                        >
                        </Column>
                        <Column
                          field="value"
                          header="Value"
                          class="align-items-center justify-content-center text-center"
                        >
                        </Column>
                      </DataTable>
                    </div>
                  </div>
                </Sidebar>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
  </div>
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <!-- // v-if="isChirlden" -->
        <div class="field col-12 md:col-12 pb-2" v-if="isChirlden">
          <label class="col-3 text-left p-0"
            >Cấp cha<span class="redsao"></span
          ></label>
          <TreeSelect
            spellcheck="false"
            selectionMode="single"
            v-model="dropdownSel"
            @change="changeCateSelect"
            class="col-8 ip36 p-0"
            :options="treeCateSelect"
            placeholder="Select Parent"
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
            :class="{ 'p-invalid': v$.category_name.$invalid && submitted }"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.category_name.$invalid && submitted) ||
              v$.category_name.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v$.category_name.required.$message
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
        @click="closeDialog"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveCategory(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerAPI"
    v-model:visible="displayAPI"
    :style="{ width: '40vw' }"
    v-on:hide="reloadService(false)"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12" v-if="isSaveService">
          <label class="col-2 text-left p-0">Loại thư viện</label>
          <TreeSelect
            spellcheck="false"
            selectionMode="single"
            v-model="dropdownSel"
            @change="changeSerSelect"
            class="col-10 ip36 p-0"
            :options="treeCateSelect"
            placeholder="Select Parent"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Tên API <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="service.service_name"
            spellcheck="false"
            class="col-10 ip36 px-2"
            :class="{
              'p-invalid': validateService.service_name.$invalid && submitted,
            }"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-2 text-left"></div>
          <small
            v-if="
              (validateService.service_name.$invalid && submitted) ||
              validateService.service_name.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validateService.service_name.required.$message
                .replace("Value", "Tên API")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Tên thủ tục </label>
          <InputText
            v-model="service.proc_name"
            spellcheck="false"
            class="col-10 ip36 px-2"
          />
        </div>
        <div style="display: flex" class="col-12 field md:col-12">
          <div class="field col-4 md:col-4 p-0">
            <label class="col-6 text-left p-0">Số thứ tự </label>
            <InputNumber v-model="service.is_order" class="col-6 ip36 p-0" />
          </div>
          <div class="field col-4 md:col-4 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >App</label
            >
            <InputSwitch v-model="service.is_app" class="col-6" />
          </div>
          <div class="field col-4 md:col-4 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Trạng thái
            </label>
            <InputSwitch v-model="service.status" class="col-6" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Mô tả</label>
          <div class="col-10 p-0">
            <!-- <Editor v-model="service.des" editorStyle="height: 150px"/> -->
            <Textarea
              spellcheck="false"
              v-model="service.des"
              class="col-12 ip36 p-2"
              autoResize
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Dữ liệu trả về</label>
          <div class="col-10 p-0">
            <Textarea
              spellcheck="false"
              v-model="service.data"
              class="col-12 ip36 p-2"
              autoResize
            />
            <!-- <Editor v-model="service.data" editorStyle="height: 150px" /> -->
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Từ khóa</label>
          <div class="col-10 p-0 m-0">
            <Chips v-model="service.keywords" class="p-0 w-full m-0" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">File</label>
          <div class="col-10 p-0 m-0">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="false"
              accept=".cs,.txt"
              :maxFileSize="10000000"
              @select="onUploadFile"
              @remove="removeFile"
            />
          </div>
        </div>
        <div class="col-12 p-0 flex field" v-if="service.url_file">
          <label class="col-2 text-left"></label>
          <div class="col-10 p-0">
            <div class="flex">
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <img
                      src="/src/assets/image/filess.png"
                      style="object-fit: contain"
                      width="50"
                      height="50"
                      alt="logorar"
                    />
                    <span style="line-height: 50px">
                      {{ service.url_file.substring(17) }}</span
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

        <div style="display: flex" class="col-12 field md:col-12">
          <div class="field col-8 md:col-8 p-0">
            <label class="col-3 text-left p-0">Phương thức</label>
            <Dropdown
              v-model="service.method"
              :options="DrdMethod"
              optionLabel="label"
              optionValue="value"
              placeholder="Chọn phương thức"
              class="p-0"
              style="width: 15rem"
            />
          </div>
          <div
            class="field col-4 md:col-4 p-0"
            style="
              display: flex;
              justify-content: center;
              align-items: center;
              vertical-align: middle;
              text-align: center;
            "
          >
            <Checkbox
              v-model="service.isFormData"
              :binary="true"
              style="margin-left: 16px"
            />
            <label style="place-content: center" class="p-2"> Formdata </label>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeAPI()"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveAPI(!validateService.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerParameter"
    v-model:visible="displayParameter"
    :style="{ width: '40vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Tên tham số <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="parameter.parameters_name"
            spellcheck="false"
            class="col-10 ip36 px-2"
            :class="{
              'p-invalid':
                validateParameter.parameters_name.$invalid && submitted,
            }"
          />
        </div>

        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-2 text-left"></div>
          <small
            v-if="
              (validateParameter.parameters_name.$invalid && submitted) ||
              validateParameter.parameters_name.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validateParameter.parameters_name.required.$message
                .replace("Value", "Tên tham số")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Kiểu dữ liệu <span class="redsao">(*)</span></label
          >
          <Dropdown
            v-model="parameter.parameters_type"
            :options="CsharpType"
            optionLabel="name"
            optionValue="name"
            placeholder="Chọn kiểu dữ liệu hoặc nhập"
            :editable="true"
            :filter="true"
            spellcheck="false"
            class="col-10 ip36 p-0"
          >
          </Dropdown>
        </div>

        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-2 text-left"></div>
          <small
            v-if="
              (validateParameter.parameters_type.$invalid && submitted) ||
              validateParameter.parameters_type.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validateParameter.parameters_type.required.$message
                .replace("Value", "Kiểu dữ liệu ")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div style="display: flex" class="col-12 field md:col-12">
          <label class="col-2 text-left p-0">Bảng</label>
          <Dropdown
            v-model="parameter.table_id"
            :options="listTable"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn bảng(nếu có)"
            :filter="true"
            spellcheck="false"
            class="col-10 ip36 p-0"
          >
          </Dropdown>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Mô tả</label>
          <div class="col-10 p-0">
            <!-- <Editor v-model="service.des" editorStyle="height: 150px"/> -->
            <Textarea
              v-model="parameter.des"
              class="col-12 ip36 p-2"
              autoResize
              spellcheck="false"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Ví dụ</label>
          <InputText
            v-model="parameter.example_value"
            spellcheck="false"
            class="col-10 ip36 px-2"
          />
        </div>

        <div style="display: flex" class="col-12 field md:col-12">
          <div class="field col-6 md:col-6 p-0">
            <label class="col-4 text-left p-0">Số thứ tự </label>
            <InputNumber v-model="parameter.is_order" class="col-8 ip36 p-0" />
          </div>

          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Trạng thái
            </label>
            <InputSwitch v-model="parameter.status" class="col-6" />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeParameter()"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveParameter(!validateParameter.$invalid)"
      />
      <!-- -->
    </template>
  </Dialog>
  <Dialog
    v-model:visible="isShowAddBug"
    :style="{ width: '40vw' }"
    :header="headerAddBug"
  >
    <!-- @hide="reloadPlugin" -->
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Tên lỗi <span class="redsao">(*)</span></label
          >
          <InputText
            class="col-10 ip36 p-0 m-0"
            v-model="bug.bug_name"
            required="true"
            autofocus
            :class="{
              'p-invalid': validateBug.bug_name.$invalid && submitted,
            }"
          />
        </div>

        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-2 text-left"></div>
          <small
            v-if="
              (validateBug.bug_name.$invalid && submitted) ||
              validateBug.bug_name.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validateBug.bug_name.required.$message
                .replace("Value", "Tên lỗi")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Mô tả</label>
          <div class="col-10 p-0">
            <Editor v-model="bug.des" editorStyle="height: 150px" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Trạng thái</label>
          <Dropdown
            v-model="bug.status"
            :options="listStatus"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn trạng thái"
            spellcheck="true"
            class="col-10 ip36 p-0"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Từ khóa</label>
          <Chips v-model="bug.keyword" class="p-0 w-full m-0" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">File lỗi</label>
          <div class="col-10 p-0 m-0">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="true"
              accept=".zip,.rar"
              :maxFileSize="100000"
              @select="onUploadFileBug"
              @remove="removeFileBug"
            >
            </FileUpload>
          </div>
        </div>
        <div class="col-12 p-0 flex field">
          <label class="col-2 text-left"></label>
          <div class="col-10 p-0" v-if="bug.url_file">
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
                    {{ bug.url_file.substring(16) }}</span
                  >
                </div>
              </template>
              <template #end>
                <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="deleteFileBug(item)"
                />
              </template>
            </Toolbar>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeBug()"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveBug(!validateBug.$invalid)"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.method {
  color: white;
}
.d-lang-table {
  height: calc(100vh - 52px);
}
.d-table-container {
  height: calc(100vh - 500px);
}
.d-btn-function {
  border-radius: 50%;
  margin-left: 6px;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
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

.row.true {
  background-color: rgb(190, 211, 245) !important;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
}

.comment-height-scroll {
  height: calc(100vh - 300px);
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }
}
</style>
