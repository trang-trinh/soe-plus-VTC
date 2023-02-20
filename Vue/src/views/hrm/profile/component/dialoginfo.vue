<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
import moment from "moment";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  profile_id: String,
  isType: Number,
  initData: Function,
});

//Declare
const submitted = ref(false);
const datas = ref([]);
const dictionarys = ref([]);

//Function
const save2 = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  datas.value.forEach((x) => {
    if (x["identification_date_issue"] != null) {
      x["identification_date_issue"] = moment(
        x["identification_date_issue"]
      ).format("YYYY-MM-DDTHH:mm:ssZZ");
    }
    if (x["start_date"] != null) {
      x["start_date"] = moment(x["start_date"]).format("YYYY-MM-DDTHH:mm:ssZZ");
    }
    if (x["end_date"] != null) {
      x["end_date"] = moment(x["end_date"]).format("YYYY-MM-DDTHH:mm:ssZZ");
    }
  });
  formData.append("profile_id", props.profile_id);
  formData.append("relative", JSON.stringify(datas.value));
  axios
    .put(
      baseURL + "/api/hrm_profile/update_profile_relative_late",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success(
        props.isAdd ? "Thêm mới thành công!" : "Cập nhật thành công!"
      );
      props.closeDialog();
      props.initData(true);
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
  if (submitted.value) submitted.value = true;
};
const saveModel = () => {
  if (props.isType === 1) {
  } else if (props.isType === 2) {
    save2();
  }
};
const addRow = (type) => {
  datas.value.push({});
};

//init
const initDictionary2 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          dictionarys.value = tbs;
        }
      }
    })
    .then(() => {
      initView2(true);
    });
};
const initView2 = (rf) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_relative_late",
            par: [{ par: "profile_id", va: props.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((x) => {
            if (x["identification_date_issue"] != null) {
              x["identification_date_issue"] = new Date(
                x["identification_date_issue"]
              );
            }
            if (x["start_date"] != null) {
              x["start_date"] = new Date(x["start_date"]);
            }
            if (x["end_date"] != null) {
              x["end_date"] = new Date(x["end_date"]);
            }
          });
          datas.value = tbs[0];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
const initData = () => {
  if (props.isType === 1) {
  } else if (props.isType === 2) {
    initDictionary2();
  }
};
onMounted(() => {
  initData();
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '72vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <Toolbar class="w-full custoolbar font-bold">
            <template #start></template>
            <template #end>
              <a
                @click="
                  addRow(2);
                  $event.stopPropagation();
                "
                class="hover"
                v-tooltip.top="'Thêm mới'"
              >
                <i
                  class="pi pi-plus-circle"
                  data-v-62364173=""
                  style="font-size: 18px"
                ></i>
              </a>
            </template>
          </Toolbar>
        </div>
        <div class="col-12 md:col-12">
          <div style="min-height: 250px">
            <DataTable
              :value="datas"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              scrollDirection="both"
              class="empty-full"
            >
              <Column
                field="relative_name"
                header="Họ tên"
                headerStyle="text-align:center;width:180px;height:50px"
                bodyStyle="text-align:center;width:180px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.relative_name"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="25"
                  />
                </template>
              </Column>
              <Column
                field="relationship_id"
                header="Quan hệ"
                headerStyle="text-align:center;width:170px;height:50px"
                bodyStyle="text-align:center;width:170px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <Dropdown
                      :showClear="true"
                      :options="dictionarys[11]"
                      optionLabel="relationship_name"
                      optionValue="relationship_id"
                      placeholder="Chọn quan hệ"
                      v-model="slotProps.data.relationship_id"
                      class="ip36"
                      style="
                        white-space: nowrap;
                        overflow: hidden;
                        text-overflow: ellipsis;
                      "
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="identification_date_issue"
                header="Năm sinh"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.identification_date_issue"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="phone"
                header="SĐT"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputMask
                    v-model="slotProps.data.phone"
                    mask="9999999999"
                    placeholder="__________"
                    class="ip36"
                  />
                </template>
              </Column>
              <Column
                field="tax_code"
                header="Mã số thuế"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.tax_code"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="50"
                  />
                </template>
              </Column>
              <Column
                field="identification_citizen"
                header="CCCD/Hộ chiếu"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.identification_citizen"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="50"
                  />
                </template>
              </Column>
              <Column
                field="identification_date_issue"
                header="Ngày cấp"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.identification_date_issue"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="identification_place_issue"
                header="Nơi cấp"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.identification_place_issue"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="250"
                  />
                </template>
              </Column>
              <Column
                field="is_dependent"
                header="Phụ thuộc"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <Dropdown
                      :options="[
                        { value: 1, title: 'Có phụ thuộc' },
                        { value: 0, title: 'Không phụ thuộc' },
                      ]"
                      :filter="false"
                      :showClear="true"
                      :editable="false"
                      v-model="slotProps.data.is_dependent"
                      optionLabel="title"
                      optionValue="value"
                      placeholder="Chọn trạng thái"
                      class="ip36"
                      style="
                        white-space: nowrap;
                        overflow: hidden;
                        text-overflow: ellipsis;
                      "
                    >
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.title }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </template>
              </Column>
              <Column
                field="start_date"
                header="Từ ngày"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.start_date"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="end_date"
                header="Đến ngày"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.end_date"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="info"
                header="Thông tin cơ bản"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.info"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                  />
                </template>
              </Column>
              <Column
                field="note"
                header="Ghi chú"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.note"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                  />
                </template>
              </Column>
              <template #empty>
                <div
                  class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                    m-auto
                  "
                  style="display: flex; width: 100%; min-height: 200px"
                ></div>
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
        @click="props.closeDialog()"
        class="p-button-text"
      />

      <Button label="Lưu" icon="pi pi-check" @click="saveModel()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
</style>
<style lang="scss" scoped>
::v-deep(.empty-full) {
  .p-datatable-emptymessage td {
    width: 100% !important;
  }
}
</style>