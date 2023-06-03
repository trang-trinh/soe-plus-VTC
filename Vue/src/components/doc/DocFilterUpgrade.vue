<script setup>
import { ref, inject, onMounted,defineProps } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../util/function";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const emitter = inject("emitter");
const props = defineProps({
  Type: String
});
// get emit
emitter.on("emitData", (obj) => {
  switch (obj.type) {
        case "onPageFilterDoc":
            if (obj.data) {
               options.value.PageNo = obj.data.page_no;
               options.value.PageSize = obj.data.page_size;
               filterDoc();
            }
            break;
        case "onChangeCountFilterDoc":
            if (obj.data) {
                options.value.typeCount = obj.data.selected_count;
               filterDoc();
            }
            break;
            case "onRefreshFilterDoc":
            reFilter();
            reOptions();
            break;
        default: break;
    }
});
const toast = useToast();
const options = ref({
  loading: false,
  organization_id: store.getters.user.organization_id,
  user_key: store.getters.user.user_key,
  typeCount: 0,
  PageNo: 0,
  PageSize: 20,
  totalL: 0,
  sort: "fl.send_date desc, do.doc_master_id",
  search: "",
});
const reOptions = () => {
  options.value = {
  loading: false,
  organization_id: store.getters.user.organization_id,
  user_key: store.getters.user.user_key,
  typeCount: 0,
  PageNo: 0,
  PageSize: 20,
  totalL: 0,
  sort: "fl.send_date desc, do.doc_master_id",
  search: "",
};
};
// Defined init
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const listDefinedFilter = ref([]);
const reFilter = () => {
  listDefinedFilter.value = [
    {column: 'compendium', text: 'Trích yếu nội dung', type:'multiselect',value_field: 'compendium', label_field: 'compendium', options: 'compendiums', place_holder: 'Chọn trích yếu nội dung' },
    {column: 'doc_code', text: 'Số ký hiệu', type:'multiselect',value_field: 'doc_code', label_field: 'doc_code', options: 'doc_codes', place_holder: 'Chọn số ký hiệu' },
    {column: 'send_by', text: 'Người gửi', type:'multiselect',value_field: 'user_key', label_field: 'full_name', options: 'send_bys', place_holder: 'Chọn người gửi', type_doc:'receive' },
    {column: 'receive_by', text: 'Người nhận', type:'multiselect',value_field: 'user_key', label_field: 'full_name', options: 'send_bys', place_holder: 'Chọn người nhận', type_doc:'send'  },
    {column: 'send_date', text: 'Ngày nhận văn bản', type:'date', label_field: 'send_date', type_doc:'receive' },
    {column: 'send_date', text: 'Ngày gửi văn bản', type:'date', label_field: 'send_date', type_doc:'send' },
    {column: 'doc_date', text: 'Ngày văn bản', type:'date', label_field: 'doc_date' },
    {column: 'doc_status_id', text: 'Trạng thái văn bản', type:'multiselect',value_field: 'status_id', label_field: 'status_name', options: 'doc_status', place_holder: 'Chọn trạng thái văn bản' },
    {column: 'doc_group', text: 'Nhóm văn bản', type:'multiselect',value_field: 'doc_group_name', label_field: 'doc_group_name', options: 'doc_groups', place_holder: 'Chọn nhóm văn bản' },
    {column: 'issue_place', text: 'Nơi ban hành', type:'editableselect',value_field: 'issue_place_name', label_field: 'issue_place_name', options: 'issue_places', place_holder: 'Chọn nơi ban hành', can_fulltextsearch: true },
    {column: 'field_name', text: 'Lĩnh vực', type:'multiselect',value_field: 'field_name', label_field: 'field_name', options: 'field_names', place_holder: 'Chọn lĩnh vực' },
    {column: 'dispatch_book_id', text: 'Khối cơ quan', type:'multiselect',value_field: 'dispatch_book_id', label_field: 'dispatch_book_name', options: 'dispatch_books', place_holder: 'Chọn khối cơ quan' },
    {column: 'department_id', text: 'Phòng ban người soạn thảo', type:'multiselect',value_field: 'organization_id', label_field: 'organization_name', options: 'departments', place_holder: 'Chọn phòng ban người soạn thảo' },
    {column: 'tags', text: 'Từ khoá', type:'tag', place_holder: 'Ấn Enter sau mỗi từ khóa' },
    {column: 'hold_date', text: 'Thời gian tổ chức', type:'date', label_field: 'hold_place' },
    {column: 'hold_place', text: 'Địa điểm tổ chức', type:'tag', place_holder: 'Ấn Enter sau mỗi địa điểm' },
]
}
//Init category
const category = ref({});
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str:
          encr(JSON.stringify(
            {
              proc: "doc_get_dictionary",
              par: [
                { par: "organization_id", va: store.getters.user.organization_id },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "user_key", va: store.getters.user.user_key },
                { par: "typeCount", va: options.value.typeCount },
              ],
            }
          ),
            SecretKey, cryoptojs)
            .toString()
      },
      config
    )
    .then((response) => {
      debugger
      var data = response.data.data;
      if (data != null) {
        let tbn = JSON.parse(data);
        category.value.compendiums = tbn[0];
        category.value.doc_codes = tbn[1];
        category.value.send_bys = tbn[2];
        category.value.doc_status = tbn[3];
        category.value.doc_groups = tbn[4];
        category.value.issue_places = tbn[5];
        category.value.field_names = tbn[6];
        category.value.dispatch_books = tbn[7];
        category.value.departments = tbn[8];
      }
    })
    .catch((error) => {
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const change_date = (model,property) => {
  if (model['start_' + property] && model['end_' + property] && model['start_' + property] > model['end_' + property]) {
    model['end_' + property] = null;
    swal.fire({
      title: "Error!",
      text: "Ngày bắt đầu không được nhỏ hợn ngày kết thúc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
};
// Defined Dynamic
const query_dynamic = ref();
const fulltext_search = ref();
const genQueryDynamicFilter = function (is_fulltextsearch) {
  query_dynamic.value = "";
  fulltext_search.value = "";
  if(options.value.search){
      let search = options.value.search;
      if(is_fulltextsearch){
        fulltext_search.value += "(" + "CONTAINS(compendium,'\"*" + search + "*\"')" + " or " + "(dispatch_book_code like N'" + search + "')" + " or " + "CONTAINS(doc_code,'\"*" + search + "*\"')" + " or " + "CONTAINS(issue_place,'\"*" + search + "*\"')" + ")";
      }
      else{
        query_dynamic.value += "(" + "(compendium like N'%" + search + "%')" + " or " + "(dispatch_book_code like N'" + search + "')" + " or " + "(doc_code like N'%" + search + "%')" + " or " + "(issue_place like N'%" + search + "%')" + ")"
      }
  }
  let filter_used = listDefinedFilter.value.filter(x=>x.value && x.value.length > 0 || x['start_' + x.label_field] || x['end_' + x.label_field]);
  if(query_dynamic.value && filter_used.length > 0){
    query_dynamic.value += " and ";
  }
  filter_used.forEach(function (r) {
                    switch (r.type) {
                        case 'date':
                            if (!r['start_' + r.label_field] && r['end_' + r.label_field])
                            query_dynamic.value += 'cast(' + r.column + ' as date)' + " <= '" + moment(r['end_' + r.label_field]).format("YYYY-MM-DD") + "'";
                            else if (r['start_' + r.label_field] && !r['end_' + r.label_field])
                            query_dynamic.value += 'cast(' + r.column + ' as date)' + " >= '" + moment(r['start_' + r.label_field]).format("YYYY-MM-DD") + "'";
                            else if (r['start_' + r.label_field] && r['end_' + r.label_field])
                            query_dynamic.value += 'cast(' + r.column + ' as date)' + " >= '" + moment(r['start_' + r.label_field]).format("YYYY-MM-DD") + "' and " + 'cast(' + r.column + ' as date)' + " <= '" + moment(r['end_' + r.label_field]).format("YYYY-MM-DD") + "'";
                            break;
                        case 'tag':
                            r.value.forEach(function(str,idx){
                              query_dynamic.value += r.column + " like " + "N'%" + str + "%'";
                              if(idx !== r.value.length - 1) query_dynamic.value += " or ";
                            })
                            break;
                        case 'editableselect':
                          if(r.can_fulltextsearch){
                            if(fulltext_search.value) fulltext_search.value += " or ";
                            fulltext_search.value += "CONTAINS(" + r.column + ",'\"*" + r.value + "*\"')";
                          }
                          else{
                            query_dynamic.value += r.column + " like " + "N'%" + r.value + "%'";
                          }
                          break;
                        default:
                            r.value.forEach(function(str,idx){
                              query_dynamic.value += r.column + " like " + "N'%" + str + "%'";
                              if(idx !== r.value.length - 1) query_dynamic.value += " or ";
                            })
                            break;
                    };
                    if (r.column !== filter_used[filter_used.length - 1].column) query_dynamic.value += " and ";
                })
}
const filterDoc = (is_fulltextsearch) => {
  let url = "";
  switch(props.Type){
    case 'receive':
    url = 'doc_master_list_receive_dynamic';
    break;
    case 'send':
    url = 'doc_master_list_send_dynamic';
    break;
    case 'store':
    url = 'doc_master_list_store_dynamic';
    break;
  }
  genQueryDynamicFilter(is_fulltextsearch);
  showFilter.value = fulltext_search.value || query_dynamic.value;
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
        proc: url,
        par: [
          { par: "organization_id", va: options.value.organization_id },
          { par: "user_key", va: options.value.user_key },
          { par: "typeCount", va: options.value.typeCount },
          { par: "pageno", va: options.value.PageNo },
          { par: "pagesize", va: options.value.PageSize },
          { par: "fulltext_search", va: fulltext_search.value },
          { par: "dynamicquery", va: query_dynamic.value },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data != null) {
        let tbs = data
        if (tbs[0] != null && tbs[0].length > 0) {
          returnDocFilter(tbs[0]);
        } else {
          returnDocFilter([]);
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          returnCountDocFilter(tbs[1]);
        } else {
          returnCountDocFilter([]);
        }
        swal.close();
        op.value.hide();
        if (isFirst.value) isFirst.value = false;
        if (options.value.loading) options.value.loading = false;
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
}
const returnDocFilter = (docs) => {
  emitter.emit("emitData", { type: "loadFilterDoc", data: docs });
}
const returnCountDocFilter = (count) => {
  emitter.emit("emitData", { type: "loadCountDocFilter", data: count });
}
// styler
const showFilter = ref(false);
const cancelFilter = () => {
  fulltext_search.value = null;
  query_dynamic.value = null;  
  reFilter();
  reOptions();
  filterDoc();
}
onMounted(() => {
  reFilter();
  initDictionary();
  return {};
});
</script>
<template>
     <Toolbar class="search-toolbar outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="filterDoc()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder=" Tìm kiếm"
          />
        </span>
        <Button
          @click="toggle"
          type="button"
          class="ml-2" :class="{'p-button-outlined p-button-secondary': !showFilter}"
          icon="pi pi-filter"
          aria:haspopup="true"
          aria-controls="overlay_panel"
          v-tooltip="'Bộ lọc'"
        />
        <Button v-tooltip="'Bỏ lọc'" v-if="showFilter" class="ml-2 p-button-outlined p-button-secondary" icon="pi pi-filter-slash" @click="cancelFilter" />
        <OverlayPanel
          :showCloseIcon="false"
          ref="op"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          style="width: 450px"
        >
          <div class="grid formgrid m-0">
            <div class="col-12 md:col-12 p-0">
              <div
                class="scroll-outer"
                style="width: 100%; height: 400px; overflow-y: auto"
              >
                <div class="scroll-inner">
                  <div class="grid formgrid m-0">
                    <div v-for="item in listDefinedFilter" :key="item.column" class="col-12 md:col-12 p-0">
                      <wrapper v-if="!item.type_doc || (item.type_doc && props.Type === item.type_doc)">
                        <div v-if="item.type === 'editableselect'" class="form-group">
                          <label>{{ item.text }}</label>
                          <Dropdown :filter="true" v-model="item.value" :editable="true" :placeholder="item.place_holder"
                          :options="category[item.options]" :optionValue="item.value_field" :optionLabel="item.label_field" class="ip36"
                          >
                          <template #option="slotProps">
                              <div :class="{'fw-bold': slotProps.option.level === 1}" :style="{'padding-left': slotProps.option.level > 1 ? ((slotProps.option.level-1) + 'rem') : '0'}">{{slotProps.option[item.label_field]}}</div>
                          </template>
                        </Dropdown>
                        </div>
                        <div v-if="item.type === 'multiselect'" class="form-group">
                        <label>{{ item.text }}</label>
                        <MultiSelect
                          :options="category[item.options]"
                          :filter="true"
                          :optionLabel="item.label_field"
                          :optionValue="item.value_field"
                          display="chip"
                          :placeholder="item.place_holder"
                          v-model="item.value"
                          class="ip36"
                          style="height: auto; min-height: 36px"
                        >
                        </MultiSelect>
                      </div>
                      <div v-if="item.type === 'date'" class="form-group">
                        <label>{{ item.text }}</label>
                        <div class="grid formgrid m-0">
                          <div class="col-6 md:col-6 pl-0">
                            <Calendar
                              :showIcon="true"
                              class="ip36"
                              autocomplete="on"
                              inputId="time24"
                              v-model="item['start_' + item.label_field]"
                              @date-select="change_date(item, item.label_field)"
                              @input="change_date(item, item.label_field)"
                              style="width: 100%"
                              placeholder="Từ ngày"
                            />
                          </div>
                          <div class="col-6 md:col-6 pr-0">
                            <Calendar
                              :showIcon="true"
                              class="ip36"
                              autocomplete="on"
                              inputId="time24"
                              v-model="item['end_' + item.label_field]"
                              @date-select="change_date(item, item.label_field)"
                              @input="change_date(item, item.label_field)"
                              style="width: 100%"
                              placeholder="Đến ngày"
                            />
                          </div>
                        </div>
                      </div>
                      <div v-if="item.type === 'tag'" class="form-group">
                            <label>
                              {{item.text}}
                            </label>
                            <Chips class="ip36 p-0" :placeholder="item.place_holder" v-model="item.value" />
                          </div>
                      </wrapper>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12 p-0">
              <Toolbar
                class="border-none surface-0 outline-none p-0 mt-3 w-full"
              >
                <template #start>
                  <Button
                    @click="reFilter()"
                    class="p-button-outlined"
                    label="Xóa bộ lọc"
                  ></Button>
                </template>
                <template #end>
                  <div class="d-flex" style="column-gap: 2rem"></div>
                  <Button class="mr-1" @click="filterDoc(true)" label="Lọc gần đúng"></Button>
                  <Button @click="filterDoc()" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel>
      </template>
    </Toolbar>
</template>
<style scoped>
.search-toolbar.p-toolbar{
  padding: 0;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.scroll-outer {
  visibility: hidden;
}

.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}

.p-ulchip {
  list-style: none;
  margin: 0;
  padding: 0;
}

.p-lichip {
  float: left;
}

.description {
  color: #aaa;
  font-size: 12px;
}

.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  vertical-align: center;
}
</style>
<style lang="scss" scoped>
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
    white-space: normal !important;
  }

  .p-chip-text {
    word-break: break-word !important;
  }

  .p-chip img {
    margin: 0;
  }
}
::v-deep(.p-multiselect){
  .p-multiselect-label{
    flex-wrap: wrap;
    row-gap: 0.3rem;
  }
  .p-multiselect-token{
    max-width: 100%;
  }
  .p-multiselect-token-label{
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
  }

}
.fw-bold{
  font-weight: bold;
}
</style>