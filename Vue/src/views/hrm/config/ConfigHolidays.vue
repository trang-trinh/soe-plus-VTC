<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";

import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";

import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const toast = useToast();
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  title_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});

const isFirst = ref(true);

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
  IsNext: true,
  sort: " tm.title_id DESC",
  sortDM: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 20,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});

const saveDeConfig = () => {
  let formData = new FormData();

  formData.append("hrm_config_holidays", JSON.stringify(listdatas.value));

  axios
    .put(
      baseURL + "/api/hrm_config_holidays/update_hrm_config_holidays",
      formData,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật số ngày nghỉ phép thành công!");
        loadData();
      } else {
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra vui lòng thử lại sau",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
    });
};

const listdatas = ref([]);
const loadData = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_holidays_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data) {  data.forEach((element, i) => {
          element.STT = i + 1;
        });
        listdatas.value = data;
      }
    })
    .catch(() => {
      options.value.loading = false;
    });
};
const displayBasic = ref(false);
const openBasic = () => {
  loadDataTitle();
};
const closeDialog = () => {
  displayBasic.value = false;
  loadData(true);
};

const delTem = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
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
          .delete(baseURL + "/api/hrm_config_holidays/delete_hrm_config_holidays", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.config_holidays_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá số ngày nghỉ phép thành công!");
              loadData();
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

const refreshStamp = () => {
  options.value.SearchText = null;

  options.value.loading = true;

  isDynamicSQL.value = false;
  filterSQL.value = [];
  
  let filterS = {
    filterconstraints: [{ value:true, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  loadDataTitle();
};
const listDataSelected = ref([]);
const onFilter = (event) => {
  filterSQL.value = [];

  let filterS1 = {
    filterconstraints: [{ value:true, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS1);
  let filterS = {
    filterconstraints: [{ value:true, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key,
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
  options.value.pageno = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const isDynamicSQL = ref(false);
const onSort = (event) => {
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData(true);
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField == "STT") {
      options.value.sort =
        "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadDataSQL();
  }
};
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
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

    options.value.id =
      datalistsTitle.value[datalistsTitle.value.length - 1].title_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalistsTitle.value[0].title_id;
    options.value.IsNext = false;
  }
  options.value.pageno = event.page;
  
  loadDataSQL();
};
const filterSQL = ref([]);
const loadDataSQL = () => {
  datalistsTitle.value = [];

  let data = {
    id: "title_id",
    sqlS: null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    next: true,
    sqlF: null,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_ca_title", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        var stcr=[];
        data.forEach((element, i) => {
        if (
          listdatas.value.find((c) => c.title_id == element.title_id) == null
        ) {
          element.STT = options.value.pageno * options.value.pagesize + i + 1;
          if (element.title_des) {
            element.title_des = element.title_des.replaceAll("\n", "<br/>");
          }
          stcr.push(element);
        }
        
      });

        datalistsTitle.value = stcr;
      } else {
        datalistsTitle.value = [];
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

      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const sttStamp = ref();
const datalistsTitle = ref([]);
const loadDataTitle = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_title_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_title_list",
            par: [
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (isFirst.value) isFirst.value = false;
      var stcr = [];
      data.forEach((element, i) => {
        if (
          listdatas.value.find((c) => c.title_id == element.title_id) == null
        ) {
          element.STT = options.value.pageno * options.value.pagesize + i + 1;
          if (element.title_des) {
            element.title_des = element.title_des.replaceAll("\n", "<br/>");
          }
          stcr.push(element);
        }
        
      });

      datalistsTitle.value = stcr;

      options.value.loading = false;
      listDataSelected.value = [];
      displayBasic.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        store.commit("gologout");
      }
    });
};
const saveWarehouse = () => {
  let formData = new FormData();

  formData.append(
    "hrm_config_holidays",
    JSON.stringify(listDataSelected.value)
  );
  axios
    .post(
      baseURL + "/api/hrm_config_holidays/add_hrm_config_holidays",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm số ngày nghỉ phép thành công!");
        loadData();

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
};

const searchStamp = (event) => {
  if (event.code == "Enter") {
    if (options.value.SearchText == "") {
    
      options.value.loading = true;
      loadDataTitle();
    } else {
 
      options.value.loading = true;
      loadDataSQL() ;
    }
  }
};
onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadData();
 
  return {
    isFirst,
    options,
  };
});
</script>
<template>
  <div class="d-container p-0">
    <div class="p-0 surface-0">
      <!-- <div class=" w-full p-0  style-vb-1  text-center text-2xl">
       SỐ NGÀY PHÉP THƯỞNG THEO CHỨC DANH
      </div> -->

      <div class="grid mt-3">
        <div class="col-12 field p-0 font-bold">
          <div class="col-12 p-0 format-center text-xl">
            Phép thưởng theo chức danh
          </div>
        </div>
        <div class="col-12 field format-center p-0 font-bold">
          <div class="col-6 p-0 format-center text-xl">
            <div class="col-11 p-0 format-center text-xl"></div>
            <div class="col-1 p-0 format-center text-xl">
              <Button @click="openBasic" icon="pi pi-plus" />
            </div>
          </div>
        </div>
        <div class="col-12 p-0 format-center">
          <div class="col-6 p-0">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              filterMode="strict"
              :value="listdatas"
            >
              <Column
                field="is_order"
                header="STT"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:80px"
                bodyStyle="text-align:center;max-width:80px"
              >
                <template #body="data">
                  <div class="w-full">
                    <Button
                      class="w-full h-full text-center surface-200 border-1 border-400 text-900 cursor-auto"
                      :label="data.data.STT.toString()"
                    ></Button>
                  </div>
                </template>
              </Column>
              <Column
                field="info_col"
                header="Chức danh"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center"
                bodyStyle="text-align:center"
              >
                <template #body="data">
                  <div class="w-full h-full">
                    <Button
                      class="w-full h-full text-center surface-200 border-1 border-400 text-900 cursor-auto"
                      :label="data.data.title_name"
                    ></Button>
                  </div> </template
              ></Column>
              <Column
                field="separator"
                header="Số ngày thưởng"
                class="align-items-center justify-content-center text-center font-bold"
                headerStyle="text-align:center;max-width:150px"
                bodyStyle="text-align:center;max-width:150px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputNumber
                      class="w-full d-design-inputnumber"
                      style="max-width: 120px; text-align: center"
                      v-model="data.data.num_holidays" mode="decimal" :useGrouping="false" 
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="is_used"
                header=""
                class="align-items-center justify-content-center text-center font-bold"
                headerStyle="text-align:center;max-width:50px"
                bodyStyle="text-align:center;max-width:50px"
              >
                <template #body="data">
                  <Button
                    icon="pi pi-times"
                    @click="delTem(data.data)"
                  class="p-button-outlined d-designbtn-c"
                    aria-label="Submit"
                  /> </template
              ></Column>
            </DataTable>
          </div>
        </div>
        <div class="col-12 style-vb-3 py-4 text-center format-center">
          <Button @click="saveDeConfig()">Cập nhật</Button>
        </div>
        <div class="col-12 p-0 format-center style-vb-5">
          <div class="col-8 p-0 text-left align-items-center">
            <!-- <div class="w-full">
                        <font-awesome-icon
                          style="color: #ecec15"
                          icon="fa-solid fa-circle-info"
                        />
                        Bảng thiết lập người nhận phòng ban, mục đích là cấu hình người nhận mặc định của một phòng ban khi thiết bị được cấp phát cho một phòng ban.
                      </div> -->
          </div>
        </div>
      </div>
    </div>
  </div>
  <Dialog
    :maximizable="true"
    :header="'Thêm chức danh nghỉ phép thưởng'"
    v-model:visible="displayBasic"
    :modal="true"
    :style="{ width: '40vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12 p-0">
          <div class="main-layout true flex-grow-1 pb-0 pr-0">
            <DataTable
              @page="onPage($event)"
              @sort="onSort($event)"
              @filter="onFilter($event)"
              v-model:filters="filters"
              filterDisplay="menu"
              filterMode="lenient"
              :filters="filters"
              :scrollable="true"
              scrollHeight="flex"
              :showGridlines="true"
              columnResizeMode="fit"
              :lazy="true"
              :totalRecords="options.totalRecords"
              :loading="options.loading"
              :reorderableColumns="true"
              :value="datalistsTitle"
              removableSort
              v-model:rows="options.pagesize"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
              :rowsPerPageOptions="[20, 30, 50, 100, 200]"
              :paginator="true"
              dataKey="title_id"
              responsiveLayout="scroll"
              v-model:selection="listDataSelected"
              :row-hover="true"
            >
              <template #header>
                <h3 class="module-title m-0">
                  <i class="pi pi-bookmark-fill"></i> Danh sách chức danh ({{
                    options.totalRecords
                  }})
                </h3>
                <!-- <Toolbar class="w-full custoolbar">
                  <template #start>
                    <span class="p-input-icon-left">
                      <i class="pi pi-search" />
                      <InputText
                        v-model="options.SearchText"
                        @change="searchStamp"
                        type="text"
                        spellcheck="false"
                        placeholder="Tìm kiếm"
                      />
                    </span>
                  </template>

                  <template #end>
                    <Button
                      @click="refreshStamp"
                      class="mr-2 p-button-outlined p-button-secondary"
                      icon="pi pi-refresh"
                      v-tooltip="'Tải lại'"
                    />
                  </template> </Toolbar
              > -->
            </template>

              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:70px;height:50px"
                bodyStyle="text-align:center;max-width:70px"
                 selectionMode="multiple"  v-if="store.getters.user.is_super==true"
              >
              </Column>

              <Column
                field="STT"
                header="STT"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:70px;height:50px"
                bodyStyle="text-align:center;max-width:70px"
                :sortable="true"
              ></Column>

              <Column
                field="title_name"
                header="Tên chức danh"
                :sortable="true"
                headerStyle=" ;height:50px"
                bodyStyle="text-align:left"
                headerClass="align-items-center justify-content-center "
              >
                <template #filter="{ filterModel }">
                  <InputText
                    type="text"
                    v-model="filterModel.value"
                    class="p-column-filter"
                    placeholder="Từ khoá"
                  />
                </template>
              </Column>
              <Column
                field="status"
                header="Mô tả"
                headerStyle="text-align:center  ;max-width:400px;height:50px"
                bodyStyle=" max-width:400px"
                headerClass="align-items-center justify-content-center "
              >
                <template #body="data">
                  <div v-html="data.data.title_des"></div> </template
              ></Column>

              <template #empty>
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  v-if="!isFirst"
                >
                  <img
                    src="../../../assets/background/nodata.png"
                    height="144"
                  />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-outlined"
      />

      <Button label="Chọn" icon="pi pi-check" @click="saveWarehouse()" />
    </template>
  </Dialog>
</template>
<style scoped>
.d-designbtn-c:hover{
  background-color:#2196F3 !important ;
  color: white !important;
}
/* .check-scroll {
  max-height: 40rem;
  overflow: scroll;
} */

@media only screen and (max-height: 768px) {
  .style-vb-1 {
    font-size: large !important;
    padding: 8px !important;
  }

  .style-vb-2 {
    font-size: small !important;
  }

  .style-vb-3 {
    padding: 8px !important;
  }

  /* .check-scroll {
    max-height: 25rem;
    overflow: scroll;
  } */
}

@media only screen and (max-height: 678px) {
  .style-vb-5 {
    display: none;
  }

  /* .check-scroll {
    max-height: 40rem;
    overflow: scroll;
  } */
}
</style>