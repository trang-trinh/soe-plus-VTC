<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import router from "@/router";
//init Model
const tdTargets = ref([
  { value: "_blank", text: "Mở sang tab mới" },
  { value: "_self", text: "Mở tab hiện tại" },
]);
const topic = ref({
  Topic_Name: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Topic_Name: {
    required,
  },
};
const v$ = useVuelidate(rules, topic);
//Khai báo biến
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const arrroutes = ref([]);
const selectCapchaDonvi = ref();
//const treedonvis = ref();
const topicFlags = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 20,
  Lang_ID: store.getters.lang.Lang_ID,
  Donvi_ID: store.getters.user.Donvi_ID,
  user_id: store.getters.user.user_id,
});
const topics = ref();
const treetopics = ref();
const displayAddTopic = ref(false);
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
      exportTopic("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportTopic("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Topic_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Topic_ID), 1);
};
const handleFileUpload = (event, ia) => {
  files[ia] = event.target.files;
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAddTopic = () => {
  submitted.value = false;
  selectCapcha.value = {};
  topic.value = {
    Topic_Name: "",
    STT: topics.value.length + 1,
    Trangthai: true,
    Lang_ID: store.getters.lang.Lang_ID,
    Donvi_ID: store.getters.user.Donvi_ID,
    IsTarget: "_self",
  };
  displayAddTopic.value = true;
};
const closedisplayAddTopic = () => {
  displayAddTopic.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadTopic(true);
};
const onSearch = () => {
  loadTopic(true);
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
const loadTopic = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_Topic_List",
        par: [
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Search", va: opition.value.search },
          { par: "TopicFlag_ID", va: opition.value.TopicFlag_ID },
          { par: "Lang_ID", va: opition.value.Lang_ID },
          { par: "Donvi_ID", va: opition.value.Donvi_ID },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTree(data, "Parent_ID", "Topic_ID", "Topic_Name", "Biên mục");
        topics.value = obj.arrChils;
        treetopics.value = obj.arrtreeChils;
      } else {
        topics.value = [];
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
const editTopic = (md) => {
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddTopic.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "CMS_Topic_Get", par: [{ par: "Topic_ID", va: md.Topic_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        topic.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[
          topic.value.Parent_ID != null ? topic.value.Parent_ID : "-1"
        ] = true;
        selectCapchaDonvi.value = {};
        selectCapchaDonvi.value[
          topic.value.Donvi_ID != null ? topic.value.Donvi_ID : "-1"
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
  let keys = Object.keys(selectCapcha.value);
  topic.value.Parent_ID = keys[0];
  if (topic.value.Parent_ID == -1) {
    topic.value.Parent_ID = null;
  }
  if (selectCapchaDonvi.value) {
    keys = Object.keys(selectCapchaDonvi.value);
    topic.value.Donvi_ID = keys[0];
    if (topic.value.Donvi_ID == -1) {
      topic.value.Donvi_ID = null;
    }
  }
  addTopic();
};

const addTreeTopic = (md) => {
  let STT = topics.value.length + 1;
  if (md.children) {
    STT = md.children[md.children.length - 1].data.STT + 1;
  } else if (md.Cap != 0) {
    STT = 1;
  }
  selectCapcha.value = {};
  selectCapcha.value[md.data.Topic_ID] = true;
  selectCapchaDonvi.value = {};
  selectCapchaDonvi.value[md.data.Donvi_ID != null ? md.data.Donvi_ID : "-1"] = true;
  topic.value = {
    Topic_Name: "",
    STT: STT,
    Trangthai: true,
    TopicFlag_ID: md.data.TopicFlag_ID,
    Lang_ID: store.getters.lang.Lang_ID,
    Donvi_ID: store.getters.user.Donvi_ID,
    IsTarget: "_self",
  };
  submitted.value = false;
  displayAddTopic.value = true;
};
const upTrangthaiTopic = (md) => {
  let ids = [md.Topic_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Topic/Update_TrangthaiTopic",
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
        loadTopic(true);
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

const addTopic = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: topic.value.Topic_ID ? "put" : "post",
    url: baseURL + `/api/Topic/${topic.value.Topic_ID ? "Update_Topic" : "Add_Topic"}`,
    data: topic.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật Biên mục thành công!");
        loadTopic();
        closedisplayAddTopic();
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

const delTopic = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá Biên mục này không!",
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
          .delete(baseURL + "/api/Topic/Del_Topic", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Topic_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Biên mục thành công!");
              loadTopic();
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

const exportTopic = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "CMS_Topic" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH BIÊN MỤC",
        proc: "CMS_Topic_ListExport",
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
const filteredItems = ref([]);
const searchItems = (event) => {
  //in a real application, make a request to a remote url with the query and return filtered results, for demo we filter at client side
  let query = event.query;
  let filteItems = [];
  for (let i = 0; i < arrIcons.length; i++) {
    let item = arrIcons[i];
    if (item.toLowerCase().indexOf(query.toLowerCase()) != -1) {
      filteItems.push(item);
    }
  }
  filteredItems.value = filteItems;
};
const rowClass = (data) => {
  return data.Cap == 0 ? "classtinh" : data.Cap == 1 ? "classhuyen" : "classxa";
};
const loadTudien = () => {
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
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        //let obj = renderTree(data[0], "Capcha_ID", "Donvi_ID", "organization_name", "đơn vị");
        //treedonvis.value = obj.arrtreeChils;
        topicFlags.value = data[1];
      }
    })
    .catch((error) => {});
};
//Emit lang
const emitter = inject("emitter");
emitter.on("lang", (obj) => {
  loadTopic(true);
});
onMounted(() => {
  //init
  loadTopic(true);
  loadTudien();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2" v-if="store.getters.islogin">
    <TreeTable
      :value="topics"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="lenient"
      class="p-treetable-sm e-sm"
      :paginator="topics && topics.length > 20"
      :rows="20"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="topic-title mt-0 ml-1 mb-2"><i class="pi pi-list"></i> Biên mục</h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              class="mr-2"
              v-model="opition.TopicFlag_ID"
              :options="topicFlags"
              optionLabel="TopicFlag_Name"
              optionValue="TopicFlag_ID"
              :filter="true"
              placeholder="Chọn loại biên mục"
              :show-clear="true"
              @change="loadTopic(true)"
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
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />
            <Button
              label="Xoá"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
              v-if="selectedNodes.length > 0"
              @click="delTopic()"
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
              label="Thêm Biên mục"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddTopic"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        field="Topic_ID"
        :sortable="true"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
      </Column>
      <Column field="Topic_Name" header="Tên Biên mục" :sortable="true" :expander="true">
        <template #body="md">
          <span :class="'topic' + md.node.data.Cap">{{ md.node.data.Topic_Name }}</span>
        </template>
      </Column>
      <Column
        field="STT"
        :sortable="true"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      ></Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:110px"
        bodyStyle="text-align:center;max-width:110px"
      >
        <template #body="md">
          <Checkbox
            v-model="md.node.data.Trangthai"
            :binary="true"
            @change="upTrangthaiTopic(md.node.data)"
          />
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
            icon="pi pi-plus-circle"
            class="p-button-rounded p-button-sm p-button-success"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Thêm Biên mục'"
            @click="addTreeTopic(md.node)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editTopic(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delTopic(md.node.data)"
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
    </TreeTable>
  </div>
  <Dialog
    header="Cập nhật Biên mục"
    v-model:visible="displayAddTopic"
    :style="{ width: '640px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="topic.Topic_Name"
            :class="{ 'p-invalid': v$.Topic_Name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Topic_Name.$invalid && submitted) || v$.Topic_Name.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Topic_Name.required.$message
                .replace("Value", "Tên Biên mục")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp cha</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapcha"
            :options="treetopics"
            :showClear="true"
            placeholder=""
            optionLabel="data.Topic_Name"
            optionValue="data.Topic_ID"
          ></TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại</label>
          <Dropdown
            class="col-10"
            v-model="topic.TopicFlag_ID"
            :options="topicFlags"
            optionLabel="TopicFlag_Name"
            optionValue="TopicFlag_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Link</label>
          <Dropdown
            class="col-10 ip36 p-0"
            v-model="topic.Url"
            :options="arrroutes"
            :filter="true"
            optionLabel="path"
            optionValue="path"
            :editable="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Kiểu mở</label>
          <Dropdown
            class="col-10"
            v-model="topic.IsTarget"
            :options="tdTargets"
            optionLabel="text"
            optionValue="value"
            placeholder="Chọn kiểu mở"
          />
        </div>
        <!-- <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Đơn vị</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapchaDonvi"
            :options="treedonvis"
            :showClear="true"
            placeholder=""
            optionLabel="data.organization_name"
            optionValue="data.Donvi_ID"
          ></TreeSelect>
        </div> -->
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="topic.STT" />
          <label class="col-2 text-right">Trạng thái</label>
          <InputSwitch style="vertical-align: text-bottom" v-model="topic.Trangthai" />
          <label class="col-2 text-right">Menu</label>
          <InputSwitch style="vertical-align: text-bottom" v-model="topic.IsMenu" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddTopic"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
</template>
<style scoped>
.classtinh {
  background-color: aliceblue;
}
span.topictrue {
  font-weight: 500;
}
</style>
