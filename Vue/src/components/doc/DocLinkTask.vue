<script setup>
import { defineProps, onMounted, ref, inject } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";

const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");
const router = inject("router");
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const listTaskLink = ref();
const listTaskLinkView = ref();
const props = defineProps({
  headerDialog: String,
  headerDialogList: String,
  id: Number,
  displayDialog: Boolean,
  closeDialog: Function,
  displayDialogList: Boolean,
  closeDialogList: Function,
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const listDropdownStatus = ref([
  {
    value: 0,
    text: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
  },
  { value: 1, text: "Đang làm", bg_color: "#2196f3", text_color: "#FFFFFF" },
  { value: 2, text: "Tạm ngừng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 3, text: "Đã đóng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 4, text: "HT đúng hạn", bg_color: "#04D215", text_color: "#FFFFFF" },
  { value: 5, text: "Đợi review", bg_color: "#33c9dc", text_color: "#FFFFFF" },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã review", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
const optionsLinkTask = ref({
  search: "",
  PageNo: 0,
  PageSize: 20,
  search: '',
  loading: true,
  totalRecords: null,
  loaicv: 1,
});
const optionsLinkTaskView = ref({
  search: "",
  PageNo: 0,
  PageSize: 20,
  search: '',
  loading: true,
  totalRecordViews: null,
  loaicv: 0,
});
const LoadLinkTaskOrigin = () => {
  optionsLinkTask.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "doc_get_list_linktask",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "loaicv", va: optionsLinkTask.value.loaicv },
          { par: "doc_master_id", va: props.id },
          { par: "search", va: optionsLinkTask.value.search },
          { par: "pageno", va: optionsLinkTask.value.PageNo },
          { par: "pagesize", va: optionsLinkTask.value.PageSize },
        ],
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[0].forEach((element, i) => {
        element.STT = optionsLinkTask.value.PageNo * optionsLinkTask.value.PageSize + i + 1;
        element.is_check = false;
      });
      listTaskLink.value = data[0];
      optionsLinkTask.value.totalRecords = data[1][0].total;
      optionsLinkTask.value.loading = false;
      swal.close();
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTask.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}
const LoadViewListLinkTaskOrigin = () => {
  optionsLinkTaskView.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "doc_get_list_linktask_view",
        par: [
          { par: "doc_master_id", va: props.id },
          { par: "search", va: optionsLinkTaskView.value.search },
          { par: "pageno", va: optionsLinkTaskView.value.PageNo },
          { par: "pagesize", va: optionsLinkTaskView.value.PageSize },
        ],
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[0].forEach((element, i) => {
        element.STT = optionsLinkTaskView.value.PageNo * optionsLinkTaskView.value.PageSize + i + 1;
      });
      listTaskLinkView.value = data[0];
      optionsLinkTaskView.value.totalRecordViews = data[1][0].total;
      optionsLinkTaskView.value.loading = false;
      swal.close();
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTaskView.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}
const onPage = (event) => {
  if (event.rows != optionsLinkTask.value.PageSize) {
    optionsLinkTask.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    optionsLinkTask.value.id = null;
    optionsLinkTask.value.IsNext = true;
  } else if (event.page > optionsLinkTask.value.PageNo + 1) {
    //Trang cuối
    optionsLinkTask.value.id = -1;
    optionsLinkTask.value.IsNext = false;
  } else if (event.page > optionsLinkTask.value.PageNo) {
    //Trang sau

    optionsLinkTask.value.id = listTaskLink.value[listTaskLink.value.length - 1].task_id;
    optionsLinkTask.value.IsNext = true;
  } else if (event.page < optionsLinkTask.value.PageNo) {
    //Trang trước
    optionsLinkTask.value.id = listTaskLink.value[0].task_id;
    optionsLinkTask.value.IsNext = false;
  }
  optionsLinkTask.value.PageNo = event.page;
  LoadLinkTaskOrigin();
};
const onPageView = (event) => {
  if (event.rows != optionsLinkTaskView.value.PageSize) {
    optionsLinkTaskView.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    optionsLinkTaskView.value.id = null;
    optionsLinkTaskView.value.IsNext = true;
  } else if (event.page > optionsLinkTaskView.value.PageNo + 1) {
    //Trang cuối
    optionsLinkTaskView.value.id = -1;
    optionsLinkTaskView.value.IsNext = false;
  } else if (event.page > optionsLinkTaskView.value.PageNo) {
    //Trang sau

    optionsLinkTaskView.value.id = listTaskLinkView.value[listTaskLinkView.value.length - 1].task_id;
    optionsLinkTaskView.value.IsNext = true;
  } else if (event.page < optionsLinkTaskView.value.PageNo) {
    //Trang trước
    optionsLinkTaskView.value.id = listTaskLinkView.value[0].task_id;
    optionsLinkTaskView.value.IsNext = false;
  }
  optionsLinkTaskView.value.PageNo = event.page;
  LoadViewListLinkTaskOrigin();
};
const saveAddLinkTask = () => {
  var models = [];
  var list = listTaskLink.value.filter(x => x.is_check);
  if (list.length <= 1) {
    list.forEach(function (l) {
      let task_linkdoc = { linkdoc_id: null, organization_id: store.getters.user.organization_id, task_id: l.task_id, doc_master_id: props.id, is_main: true };
      models.push(task_linkdoc);
    })
  } else {
    list.forEach(function (l) {
      let task_linkdoc = { linkdoc_id: null, organization_id: store.getters.user.organization_id, task_id: l.task_id, doc_master_id: props.id, is_main: false };
      models.push(task_linkdoc);
    })
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/task_origin/Add_LinkTask_Doc",
      models,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Liên kết công việc thành công!");
        props.closeDialog();
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const DelTaskLinkView = (item) => {
  swal.fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá liên kết công việc này không!",
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
        var ids = [];
        if (item != null) {
          ids = [item.linkdoc_id];
        }
        axios
          .delete(baseURL + "/api/task_origin/Delete_task_linkdoc", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá liên kết công việc thành công!");
              LoadViewListLinkTaskOrigin();
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
}
const ChangeLoaiCV = (type) => {
  optionsLinkTask.value.loaicv = type;
  LoadLinkTaskOrigin();
}
onMounted(() => {
  if (props.displayDialog) {
    LoadLinkTaskOrigin();
  }
  if (props.displayDialogList) {
    LoadViewListLinkTaskOrigin();
  }
  return {

  };
});
</script>
<template>
  <Dialog contentClass='task_list' :header="props.headerDialog" v-model:visible="props.displayDialog"
    style="z-index:10000;overflow-y: hidden !important;" :style="{ width: '600px' }" :closable="false">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12" style="display: flex;align-items: center;">
          <div style="width: 50%;">
            <ul style="display: flex;padding: 0px;">
              <li @click="ChangeLoaiCV(1)" :class="{ active: optionsLinkTask.loaicv != 0 }" style="list-style:none;">Tôi
                làm</li>
              <li @click="ChangeLoaiCV(0)" :class="{ active: optionsLinkTask.loaicv == 0 }" style="list-style:none;">
                Quản lý</li>
            </ul>
          </div>
          <div style="width: 50%;">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText style="min-width: 300px;" type="text" spellcheck="false" v-model="optionsLinkTask.search"
                placeholder="Tìm kiếm" @keyup.enter="LoadLinkTaskOrigin(datalists)" />
            </span>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <div v-if="store.getters.islogin" style="background-color:#fff !important;" class="flex-grow-1 p-2">
            <DataTable v-model:first="first" :rowHover="true" :value="listTaskLink" :scrollable="true"
              scrollHeight="flex" :totalRecords="optionsLinkTask.totalRecords" :row-hover="true" dataKey="task_id"
              :paginator="true" :rows="optionsLinkTask.PageSize"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              :rowsPerPageOptions="[20, 30, 50, 100, 200]" v-model:selection="selectedTasks" @page="onPage($event)"
              @sort="onSort($event)" @filter="onFilter($event)" :lazy="true" selectionMode="single">

              <Column headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:4rem;"
                class="align-items-center justify-content-center text-center">
                <template #body="data">
                  <Checkbox :binary="true" v-model="data.data.is_check" />
                </template>
              </Column>
              <Column header="STT" field="STT" headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:4rem;"
                class="align-items-center justify-content-center text-center">
              </Column>
              <Column header="Tên công việc" headerStyle="min-height:3.125rem" bodyStyle=" ">
                <template #body="data">
                  <div style="display:flex;flex-direction: column;padding: 5px;">
                    <div style="min-height: 25px;"><span style="font-weight: bold;font-size:14px;">{{
                        data.data.task_name
                    }}</span></div>
                    <div style="font-size:12px;">
                      <span v-if="data.data.start_date || data.data.end_date" style="color:#98a9bc;">{{
                          (data.data.start_date) ? moment(new
                            Date(data.data.start_date)).format("DD/MM/YYYY") : null
                      }} - {{ (data.data.end_date) ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
    : null
}}</span>
                    </div>
                    <div v-if="data.data.project_name" style="min-height: 25px;display: flex;align-items: center;">
                      <i class="pi pi-tag"></i>
                      <span class="duan" style="font-size: 13px;font-weight:400;margin-left:5px;color:#0078d4;">{{
                          data.data.project_name
                      }}</span>
                    </div>
                  </div>
                </template>
              </Column>
              <template #empty>
                <div class="align-items-center justify-content-center p-4 text-center m-auto"
                  style="min-height: calc(100vh - 375px);max-height: calc(100vh - 375px);display: flex;flex-direction: column;"
                  v-if="!isFirst">
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
            <!-- <DetailedWork class="main-layout true flex-grow-1 p-2" v-if="showDetail == true && selectedTaskID != null"
              :isShow="showDetail" :id="selectedTaskID" :turn="0">
            </DetailedWork> -->
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="props.closeDialog" class="p-button-text" />

      <Button label="Lưu" icon="pi pi-check" @click="saveAddLinkTask()" />
    </template>
  </Dialog>

  <Dialog contentClass='task_list_view' :header="props.headerDialogList" v-model:visible="props.displayDialogList"
    style="z-index:10000;overflow-y: hidden !important;" :style="{ width: '600px' }" :closable="false">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <div v-if="store.getters.islogin" style="background-color:#fff !important;" class="flex-grow-1 p-2">
            <DataTable v-model:first="first" :rowHover="true" :value="listTaskLinkView" :scrollable="true"
              scrollHeight="flex" :totalRecords="optionsLinkTaskView.totalRecordViews" :row-hover="true"
              dataKey="task_id" :paginator="true" :rows="optionsLinkTaskView.PageSize"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              :rowsPerPageOptions="[20, 30, 50, 100, 200]" v-model:selection="selectedTasks" @page="onPageView($event)"
              @sort="onSort($event)" @filter="onFilter($event)" :lazy="true" selectionMode="single">

              <Column header="STT" field="STT" headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:4rem;"
                class="align-items-center justify-content-center text-center">
              </Column>
              <Column header="Tên công việc" headerStyle="min-height:3.125rem" bodyStyle=" ">
                <template #body="data">
                  <div style="display:flex;flex-direction: column;padding: 5px;">
                    <div style="min-height: 25px;"><span style="font-weight: bold;font-size:14px;">{{
                        data.data.task_name
                    }}</span></div>
                    <div style="font-size:12px;">
                      <span v-if="data.data.start_date || data.data.end_date" style="color:#98a9bc;">{{
                          (data.data.start_date) ? moment(new
                            Date(data.data.start_date)).format("DD/MM/YYYY") : null
                      }} - {{ (data.data.end_date) ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
    : null
}}</span>
                    </div>
                    <div v-if="data.data.project_name" style="min-height: 25px;display: flex;align-items: center;">
                      <i class="pi pi-tag"></i>
                      <span class="duan" style="font-size: 13px;font-weight:400;margin-left:5px;color:#0078d4;">{{
                          data.data.project_name
                      }}</span>
                    </div>
                  </div>
                </template>
              </Column>
              <Column headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:4rem;"
                class="align-items-center justify-content-center text-center">
                <template #body="data">
                  <span v-if="store.getters.user.user_id == data.data.created_by"
                    @click="DelTaskLinkView(data.data)"><i class="pi pi-trash"></i></span>
                </template>
              </Column>
              <template #empty>
                <div class="align-items-center justify-content-center p-4 text-center m-auto"
                  style="min-height: calc(100vh - 375px);max-height: calc(100vh - 375px);display: flex;flex-direction: column;"
                  v-if="!isFirst">
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
            <!-- <DetailedWork class="main-layout true flex-grow-1 p-2" v-if="showDetail == true && selectedTaskID != null"
              :isShow="showDetail" :id="selectedTaskID" :turn="0">
            </DetailedWork> -->
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="props.closeDialogList" class="p-button-text" />
    </template>
  </Dialog>
</template>
<style>
.task_list {
  overflow-y: hidden !important;
}

.task_list .p-toolbar-group-left {
  flex: 1;
}

.task_list .p-input-icon-left {
  width: 100%;
}

.task_list .p-inputtext {
  min-width: 100% !important;
}

.task_list .p-datatable-tbody {
  overflow-y: auto !important;
  max-height: calc(100vh - 360px);
  min-height: calc(100vh - 360px);
}

.task_list ul li {
  min-width: 60px;
  font-size: 14px;
  font-weight: bold;
  padding: 10px;
}

.task_list ul li:hover {
  cursor: pointer;
  border-bottom: 1px solid #6c757d;
}

.task_list ul .active {
  border-bottom: 1px solid #2196F3 !important;
  color: #2196F3;
}
</style>
