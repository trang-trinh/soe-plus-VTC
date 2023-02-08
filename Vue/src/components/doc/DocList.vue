<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import { useRoute } from "vue-router";
import {formatDate} from "../../util/function";
import DocListBox from "./DocListBox.vue"
import { encr } from "../../util/function";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const props = defineProps({
    Type: String
});
const route = useRoute();
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
// reload component
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const emitter = inject("emitter");
// get emitter
emitter.on("emitData", (obj) => {
    switch (obj.type) {
      case "goToDetailDoc":
        if (obj.data) {
          selectDoc(obj.data);
        }
        break;
      case "reloadDoc":
        if (obj.data && obj.data === 'refresh') {
          onRefresh();
        }
        else {
          loadDocAll(true);
        }
        break;
      case "loadFilterDoc":
        if (obj.data) {
          docs.value = obj.data;
          if (docs.value.length > 0) {
            docs.value.forEach(function (r) {
              if (r.send_date) r.send_date = moment(r.send_date).format('DD/MM/yyyy HH:mm');
              if (props.Type === 'send') {
                if (r.receive_by_name) {
                  r.receive_by_name = r.receive_by_name.substring(0, r.receive_by_name.length - 1);
                  let arr_name = r.receive_by_name.split(',');
                  if (arr_name.length > 0) {
                    if (arr_name.length > 1) {
                      r.receive_by_name = arr_name[0] + ' và ' + (arr_name.length - 1) + ' người khác';
                    }
                    else {
                      r.receive_by_name = arr_name[0];
                    }
                  }
                }
              }
            });
            docs.value[0].isClicked = true;
          }
          forceRerender();
          is_filtered.value = true;
        }
        break;
      case "loadCountDocFilter":
        if (obj.data) {
          if (obj.data.length > 0) {
            let data = obj.data;
            defCountDoc.value.forEach(function (r) {
              r.value = data[0][r.code];
            });
            defCountAllDoc.value.value = data[0][defCountAllDoc.value.code];
            let cur_records = defCountDoc.value.find(x=>x.ord === selectedCount.value).value;
            options.value.totalRecords = cur_records;
          }
        }
        break;
      case "changeAdditionalCount":
        if(obj.data){
          selectedCount.value = obj.data;
          onChangeCountDoc(selectedCount.value);
        }
        break;
      default: break;
    }
});
// filter
const is_filtered = ref(false);
const onPageFilterDoc = () => {
  emitter.emit("emitData", { type: "onPageFilterDoc", data: {page_no: options.value.PageNo, page_size: options.value.PageSize} });
}
// defined variable
const default_page_no = 0;
const default_page_size = 20;
const options = ref({
    search: "",
    PageNo: default_page_no,
    PageSize: default_page_size,
    totalRecords: 0,
    user_key: store.getters.user.user_key,
    organization_id: store.getters.user.organization_id,
    Sort: "Ngaytao",
    Order: "asc",
    Type: 0,
})
const layout = ref("list");
const docs = ref([]);
const isFirst = ref(true);
const onPage = (event) => {
  options.value.PageNo = event.page;
  options.value.PageSize = event.rows;
  if(is_filtered.value){
    onPageFilterDoc();
  }
  else{
    loadDocAll(true);
  }
  
};
const loadDocAll = (f) => {
  if(props.Type === 'receive'){
    loadDoc(f);
  }
  else if(props.Type === 'send'){
    loadDocSend(f);
  }
  else if(props.Type === 'store'){
    loadDocStore(f);
  }
}
const onRefresh = () => {
  options.value = {
    search: "",
    PageNo: default_page_no,
    PageSize: default_page_size,
    totalRecords: 0,
    user_key: store.getters.user.user_key,
    organization_id: store.getters.user.organization_id,
    Sort: "Ngaytao",
    Order: "asc",
    Type: 0,
};
selectedCount.value = 0
is_filtered.value = false;
onRefreshFilterDoc();
loadDocAll(true);
}
const onRefreshFilterDoc = () => {
  emitter.emit("emitData", { type: "onRefreshFilterDoc", data: null });
}
const loadDoc = (rf) => {
  if (rf) {
    docs.value = [];
    options.value.loading = true;
    loadCountDoc();
    // if (options.value.PageNo == 1) loadCount();
  }
  swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_list_receive",
        par: [
          { par: "organization_id", va: options.value.organization_id },
          { par: "user_key", va: options.value.user_key },
          { par: "typeCount", va: selectedCount.value },
          { par: "pageno", va: options.value.PageNo },
          { par: "pagesize", va: options.value.PageSize },
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
        data.forEach(function (r) {
          if(r.doc_date) r.doc_date = formatDate(r.doc_date,'date');
          if(r.send_date) r.send_date = formatDate(r.send_date,'datetime');
        });
        docs.value = data;
        // check if notify
        if(isFirst.value){
          if(route.params.id){
          getDocByNotify(route.params.id);   
          }
          else{
            docs.value[0].isClicked = true;
          }
        }
        else{
          docs.value[0].isClicked = true;
        }
      } else {
        docs.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        options.value.loading = false;
      }
      swal.close();
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
// select doc
const selectDoc = (docitem) => {
  docs.value.forEach(function(it){
    if(it.doc_master_id === docitem.doc_master_id && it.follow_id === docitem.follow_id){
      it.isSelected = true;
    }
    else it.isSelected = false;
  });
}
// count doc
const selectedCount = ref(0);
const defCountDoc = ref([
  {ord: 0, text: 'Tổng số', code: 'all'},
  {ord: 1, text: 'Đến', code: 'receive'},
  {ord: 2, text: 'Đi', code: 'send'},
  {ord: 3, text: 'Nội bộ', code: 'internal'},
  {ord: 4, text: 'Đã liên kết công việc', code: 'reltask'},
  {ord: 5, text: 'Nhận chưa đọc', code: 'notseen'},
  {ord: 6, text: 'Chờ xử lý', code: 'handle'},
  {ord: 7, text: 'Quá hạn', code: 'ood'},
  {ord: 8, text: 'Giấy mời', code: 'invite'},
  {ord: 9, text: 'GQ BHXH, BHYT', code: 'insurance'},
]);
const defCountAllDoc = ref({
  ord: 0, text: 'Tổng số', code: 'all'
});
const onChangeCountDoc = (val) => {
  options.value.PageNo = default_page_no;
  options.value.PageSize = default_page_size;
  selectedCount.value = val;
  if(is_filtered.value){
    onChangeCountFilterDoc();
  }
  else{
    loadDocAll(true);
  }
}
const onChangeCountFilterDoc = () => {
  emitter.emit("emitData", { type: "onChangeCountFilterDoc", data: {selected_count: selectedCount.value} });
}
const loadCountDoc = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_receive_count",
        par: [
          { par: "user_key", va: options.value.user_key }
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
        defCountDoc.value.forEach(function (r) {
            r.value = data[0][r.code];
        });
        defCountAllDoc.value.value = data[0][defCountAllDoc.value.code];
        let cur_records = defCountDoc.value.find(x=>x.ord === selectedCount.value).value;
        options.value.totalRecords = cur_records;
        returnCountDocFilter(data);
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
// Doc Send
const loadDocSend = (rf) => {
  if (rf) {
    docs.value = [];
    options.value.loading = true;
    loadCountDocSend();
    // if (options.value.PageNo == 1) loadCount();
  }
  swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_list_send",
        par: [
          { par: "organization_id", va: options.value.organization_id },
          { par: "user_key", va: options.value.user_key },
          { par: "typeCount", va: selectedCount.value },
          { par: "pageno", va: options.value.PageNo },
          { par: "pagesize", va: options.value.PageSize },
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
        data.forEach(function (r) {
          if(r.doc_date) r.doc_date = formatDate(r.doc_date,'date');
          if(r.send_date) r.send_date = formatDate(r.send_date,'datetime');
          if(r.receive_by_name){
              r.receive_by_name = r.receive_by_name.substring(0, r.receive_by_name.length - 1);
              let arr_name = r.receive_by_name.split(',');
              if(arr_name.length > 0){
                if(arr_name.length > 1){
                  r.receive_by_name = arr_name[0] + ' và ' + (arr_name.length - 1) + ' người khác';
                }
                else{
                  r.receive_by_name = arr_name[0];
                }
              }
          }
        });
        data[0].isClicked = true;
        docs.value = data;
      } else {
        docs.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        options.value.loading = false;
      }
      swal.close();
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
const loadCountDocSend = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_send_count",
        par: [
          { par: "user_key", va: options.value.user_key }
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
        defCountDoc.value.forEach(function (r) {
            r.value = data[0][r.code];
        });
        defCountAllDoc.value.value = data[0][defCountAllDoc.value.code];
        let cur_records = defCountDoc.value.find(x=>x.ord === selectedCount.value).value;
        options.value.totalRecords = cur_records;
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
// Doc Store
const loadDocStore = (rf) => {
  if (rf) {
    docs.value = [];
    options.value.loading = true;
    loadCountDocStore();
    // if (options.value.PageNo == 1) loadCount();
  }
  swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_list_store",
        par: [
          { par: "organization_id", va: options.value.organization_id },
          { par: "user_key", va: options.value.user_key },
          { par: "typeCount", va: selectedCount.value },
          { par: "pageno", va: options.value.PageNo },
          { par: "pagesize", va: options.value.PageSize },
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
        data.forEach(function (r) {
          if(r.doc_date) r.doc_date = formatDate(r.doc_date,'date');
          if(r.send_date) r.send_date = formatDate(r.send_date,'datetime');
        });
        data[0].isClicked = true;
        docs.value = data;
      } else {
        docs.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        options.value.loading = false;
      }
      swal.close();
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
const loadCountDocStore = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_store_count",
        par: [
          { par: "user_key", va: options.value.user_key }
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
        defCountDoc.value.forEach(function (r) {
            r.value = data[0][r.code];
        });
        defCountAllDoc.value.value = data[0][defCountAllDoc.value.code];
        let cur_records = defCountDoc.value.find(x=>x.ord === selectedCount.value).value;
        options.value.totalRecords = cur_records;
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
// 
// count Checkbox
const clickCheckbox = (event)=>{
  let count = docs.value.filter(x=>x.checked);
  emitter.emit("emitData", { type: "returnCountCheckbox", data: {checked_docs: count, count: count.length} });
  event.stopPropagation();
}
const resetCheckbox = () => {
  docs.value.map(x=>x.checked = false);
}
// Reload count additional
const returnCountDocFilter = (count) => {
  emitter.emit("emitData", { type: "loadCountDocFilter", data: count });
}
// --------------------- Get doc by notify ------------------------
const getDocByNotify = (doc_master_id) => {
  var doc_noti_idx = docs.value.findIndex(x => x.doc_master_id == doc_master_id);
                        if (doc_noti_idx > -1) {
                          docs.value.unshift(docs.value.splice(doc_noti_idx, 1)[0]);
                            if (docs.value.length > 0) {
                              docs.value[0].isClicked = true;
                            };
                        }
                        else {
                            loadDocNotifyByID(doc_master_id);
                        }
}
const loadDocNotifyByID = (doc_master_id) => {
  swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_get_by_notify",
        par: [
          { par: "organization_id", va: options.value.organization_id },
          { par: "user_key", va: options.value.user_key },
          { par: "doc_master_id", va: doc_master_id },
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
        if(data[0].doc_date) data[0].doc_date = formatDate(data[0].doc_date,'date');
        if(data[0].send_date) data[0].send_date = formatDate(data[0].send_date,'datetime');
        docs.value.unshift(data[0]);
        docs.value[0].isClicked = true;
        forceRerender();
      } 
      else{
        swal.fire({
          title: "Thông báo!",
          text: "Văn bản không tồn tại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return false;
      }
      swal.close();
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
}
//---------------------- Another Doc -------------------
const anotherDoc = ref();
const openListAnotherDoc = (event, data) => {
  anotherDoc.value.toggle(event);
}
onMounted(() => {
  if(props.Type ==='receive'){
    loadDoc();
    loadCountDoc();
    }
    else if(props.Type ==='send'){
      loadDocSend();
      loadCountDocSend();
    }
    else if(props.Type ==='store'){
      loadDocStore();
      loadCountDocStore();
    }
  
  return {
    options,
    layout,
    docs,
    isFirst,
    onPage,
    defCountDoc,
    selectedCount
  };
});
</script>

<template>
<div class="container-doc-list" id="splitter-doclist">
<DataView 
class="w-full h-full ptable p-datatable-sm flex flex-column"
:lazy="true"
:value="docs" 
:layout="layout" 
:loading="options.loading"
:paginator="options.totalRecords > options.PageSize"
:rows="options.PageSize"
:totalRecords="options.totalRecords"
@page="onPage($event)"
paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
:rowsPerPageOptions="[20, 30, 50, 100, 200]"
currentPageReportTemplate=""
responsiveLayout="scroll"
:scrollable="true"
>
    <template #header>
        <toolbar class="custoolbar w-full surface-0">
            <template #start>
               <SelectButton @change="onChangeCountDoc(selectedCount)" v-model="selectedCount" :options="defCountDoc.slice(0,4)" optionLabel="text" dataKey="ord" optionValue="ord" aria-labelledby="custom">
                  <template #option="slotProps">
                      {{slotProps.option.text}} <Badge :value="slotProps.option.value" severity="danger"></Badge>
                  </template>
                </SelectButton>
                
                <Button :class="{'selected': defCountDoc.slice(8,10).find(x=>x.ord == selectedCount)}" class="p-button-outlined p-button-secondary btn-another-doc" icon="pi pi-chevron-down" iconPos="right" type="button" label="Văn bản khác" @click="openListAnotherDoc" />
                <Menu :model="defCountDoc.slice(8,10)" ref="anotherDoc" :popup="true">
                  <template #item="{item}">
                    <div @click="onChangeCountDoc(item.ord)" :class="{'selected': item.ord == selectedCount}" class="box-another-doc">
                      {{item.text}} <Badge :value="item.value" severity="danger"></Badge>
                  </div>
                  </template>
                </Menu>
            </template>
            <!-- <template #end>
                <DataViewLayoutOptions v-model="layout" />
            </template> -->
        </toolbar>
    </template>
    <template #list="slotProps">
        <div :class="{'selected-doc': slotProps.data.isSelected}" class="p-2 w-full h-full" style="background-color: #fff;">
          <DocListBox :key="componentKey" :clickCheckbox="clickCheckbox" :resetCheckbox="resetCheckbox"
          :DocItem="slotProps.data" :Type="props.Type"
        ></DocListBox>
        </div>
      </template>
    <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center"
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
    </template>
</DataView>
</div>
</template>

<style lang="scss" scoped>
.box-another-doc{
  padding: 0.75rem 1rem;
    color: #495057;
    border-radius: 0;
    transition: box-shadow 0.2s;
    cursor: pointer;

}
.box-another-doc.selected{
  background-color: #0d89ec;
  color: #fff;
}
.box-another-doc .p-badge{
  margin-left: 0.5rem;
    min-width: 1rem;
    height: 1rem;
    line-height: 1rem;
    padding-top: 0.1rem;
}
.box-another-doc:hover{
  background: #e9ecef;
}
::v-deep(.p-button.btn-another-doc){
  border: 1px solid #ced4da !important;
    color: #495057 !important;
    transition: background-color 0.2s, color 0.2s, border-color 0.2s, box-shadow 0.2s;
    border-radius: unset;
    border-left: none;
}
.btn-another-doc.selected{
  background-color: #2196F3 !important;
    color: #fff !important;
    border: 1px solid #5ca7e3 !important;
}
.container-doc-list{
    height: calc(100vh - 8.3rem);
}
::v-deep(.p-button) {
   .p-badge {
   padding-top: 0.1rem;
}
}
::v-deep(.p-dataview) {
  .p-dataview.p-dataview-list .p-dataview-header {
    padding: 0px !important;
  }
}
.selected-doc{
  background-color: #f0f8ff !important;
}
</style>