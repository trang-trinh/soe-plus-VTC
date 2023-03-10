<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";

import treeuser from "../../../components/user/treeuser.vue";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const toast = useToast();

const expandedKeys = ref([]);

const selectedUser = ref([]);

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const isFirst = ref(true);

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
  IsNext: true,
  sort: " cp.approved_group_id DESC",
  sortDM: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});

const saveDeConfig = () => {
  if (hrm_contract.value.year_fake)
    hrm_contract.value.year = hrm_contract.value.year_fake.getFullYear();
  let formData = new FormData();

  formData.append("hrm_config_contract", JSON.stringify(hrm_contract.value));
  formData.append(
    "hrm_config_contract_order",
    JSON.stringify(listDataOrder.value)
  );
  formData.append(
    "hrm_config_contract_symbol",
    JSON.stringify(listDataSymbol.value)
  );
  axios
    .put(
      baseURL + "/api/hrm_config_contract/update_config_contract",
      formData,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật số hợp đồng lao động thành công!");
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
const hrm_contract = ref({
  initialization_number: 1,
  year_fake: new Date(),
  auto_increment: false,
});
const listDataOrder = ref([]);
const listDataSymbol = ref([]);
const loadData = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_contract_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: hrm_contract.value.year_fake.getFullYear() },
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
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];

      if (data) {
        hrm_contract.value = data[0];
        hrm_contract.value.year_fake = new Date(
          "1/1/" + hrm_contract.value.year
        );
        listDataOrder.value = data1;
        data2.forEach((element, i) => {
          element.STT = i + 1;
        });
        listDataSymbol.value = data2;
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const loadContract = () => {
  listDataOrder.value = [];
 
  axios
    .put(
      baseURL + "/api/hrm_config_contract/update_data",
      {
        year: hrm_contract.value.year_fake.getFullYear(),
        organization_id: null,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

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

onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadContract();
  return {
    isFirst,
    options,
  };
});
</script>
<template>
  <div class="d-container p-0">
    <div class="p-0 surface-0">
      <div class="col-12 flex style-vb-3">
        <div class="col-2 flex justify-content-end align-items-center"></div>
        <div class="col-3 flex justify-content-end align-items-center">
          <div class="pr-2 font-bold">Năm:</div>
          <Calendar
            v-model="hrm_contract.year_fake"
            class="d-design-calendar"
            view="year"
            dateFormat="yy"
            @date-select="loadContract()"
 
          />
        </div>
        <div class="col-3 flex align-items-center format-center">
          <div class="pr-2 font-bold">Số khởi tạo:</div>
          <InputNumber
            v-model="hrm_contract.initialization_number"
            class="d-design-inputnumber"
            mode="decimal"
            :useGrouping="false"
          />
        </div>
        <div class="col-3 flex align-items-center">
          <div class="pr-2 font-bold">Tự động tăng:</div>
          <InputSwitch
            class="w-4rem lck-checked"
            v-model="hrm_contract.auto_increment"
          />
        </div>
        <div class="col-1 flex justify-content-end align-items-center"></div>
      </div>
      <div class="grid mt-3">
        <div class="col-12 field p-0 font-bold">
          <div class="col-12 p-0 format-center text-xl">Bảng thiết lập</div>
        </div>
        <div class="col-12 p-0 format-center">
          <div class="col-6 p-0">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              scrollHeight="flex"
              filterMode="strict"
              :value="listDataOrder"
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
                    <InputNumber
                      inputStyle="text-align:center"
                      class="w-full"
                      v-model="data.data.is_order"
                      :min="1"
                      :max="3"
                    
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="info_col"
                header="Trường thông tin"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center"
                bodyStyle="text-align:center"
              >
                <template #body="data">
                  <div class="w-full h-full">
                    <Button
                      class="w-full h-full text-center surface-200 border-1 border-400 text-900 cursor-auto"
                      :label="data.data.info_col"
                    ></Button>
                  </div> </template
              ></Column>
              <Column
                field="separator"
                header="Ký tự ngăn cách"
                class="align-items-center justify-content-center text-center font-bold"
                headerStyle="text-align:center;max-width:200px"
                bodyStyle="text-align:center;max-width:200px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputText
                      class=""
                      style="max-width: 120px; text-align: center"
                      v-model="data.data.separator"
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="is_used"
                header="Sử dụng"
                class="align-items-center justify-content-center text-center font-bold"
                headerStyle="text-align:center;max-width:100px"
                bodyStyle="text-align:center;max-width:100px"
              >
                <template #body="data">
                  <InputSwitch
                    class="w-4rem lck-checked"
                    v-model="data.data.is_used"
                  ></InputSwitch> </template
              ></Column>
            </DataTable>
          </div>
        </div>
      </div>

      <div class="grid mt-3">
        <div class="col-12 field p-0 font-bold">
          <div class="col-12 p-0 format-center text-xl">Bảng ký hiệu</div>
        </div>
        <div class="col-12 p-0 format-center">
          <div class="col-6 p-0">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              scrollHeight="flex"
              filterMode="strict"
              :value="listDataSymbol"
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
                header="Tên loại hợp đồng"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center"
                bodyStyle="text-align:center"
              >
                <template #body="data">
                  <div class="w-full h-full">
                    <Button
                      class="w-full h-full text-center surface-200 border-1 border-400 text-900 cursor-auto"
                      :label="data.data.type_contract_name"
                    ></Button>
                  </div> </template
              ></Column>
              <Column
                field="separator"
                header="Ký hiệu"
                class="align-items-center justify-content-center text-center font-bold"
                headerStyle="text-align:center;max-width:200px"
                bodyStyle="text-align:center;max-width:200px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputText
                      class=""
                      style="max-width: 120px; text-align: center"
                      v-model="data.data.symbol"
                    />
                  </div>
                </template>
              </Column>
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
</template>
<style scoped>
.check-scroll {
  max-height: 40rem;
  overflow: scroll;
}

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

  .check-scroll {
    max-height: 25rem;
    overflow: scroll;
  }
}

@media only screen and (max-height: 678px) {
  .style-vb-5 {
    display: none;
  }

  .check-scroll {
    max-height: 40rem;
    overflow: scroll;
  }
}
</style>