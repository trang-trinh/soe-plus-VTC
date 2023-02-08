
<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import { encr } from "../../util/function.js";
import detailsHandover from "./detailsHandover.vue";
import detailsRecall from "./detailsRecall.vue";
import detailsInventory from "./detailsInventory.vue";
const emitter = inject("emitter");
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
  IsNext: true,
  sort: "card_id DESC",
  sortDM: "device_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 20,
  loading: true,
  totalRecords: null,
  start_date: null,
  end_date: null,
});
const toast = useToast();
const props = defineProps({
  device: Object,
});
const loadData = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_card_details_all",
            par: [{ par: "card_id", va: props.device.card_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0][0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      let data3 = JSON.parse(response.data.data)[3];
      let data4 = JSON.parse(response.data.data)[4];
      deviceCard.value = data;

      listHandover.value = data1;
      data2.forEach((element) => {
        element.details_card = JSON.parse(element.details_card)[0];
      });
      listRepair.value = data2;
      listRecall.value = data3;
      listInventory.value = data4;
      options.value.loading = false;
      isShow.value = true;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const listHandover = ref([]);
const listRecall = ref([]);
const listRepair = ref([]);
const listInventory = ref([]);
const deviceCard = ref();
const isShow = ref(false);
onMounted(() => {
  loadData();
  return {};
});
</script>
<template>
  <form v-if="isShow" style="min-height: 50% !important">
    <div class="grid formgrid m-2 flex">
      <div class="col-3 field relative">
        <Image
          v-if="deviceCard.image"
          image-style="object-fit: cover; border: unset; outline: unset"
          imageClass="w-full p-3 max-h-10rem"
          v-bind:src="
            deviceCard.image
              ? basedomainURL + deviceCard.image
              : basedomainURL + '/Portals/Image/noimg.jpg'
          "
          @error="
            $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
          "
          preview
        />
      </div>
      <div class="col-9 field">
        <TabView class="tabview-custom" ref="tabview4">
          <TabPanel>
            <template #header>
              <i class="pi pi-tag pr-2"></i>
              <span>Thẻ thiết bị</span>
            </template>
            <div class="grid">
              <div class="col-12 field p-0 pt-2">
                <span class="text-xl font-bold"> Thông tin thẻ </span>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Số hiệu:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.device_number }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Mã barcode:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.barcode_id }}
                  </div>
                </div>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Trạng thái:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.device_status_name }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Ngày mua:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{
                      moment(new Date(deviceCard.purchase_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                  </div>
                </div>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Người lập thẻ:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.created_name }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Ngày lập thẻ:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{
                      moment(new Date(deviceCard.created_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                  </div>
                </div>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Nhà sản xuất:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.producer }}
                  </div>
                </div>

                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Nước sản xuất:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.manufacture_country }}
                  </div>
                </div>
              </div>
              <div class="col-12 p-0 field pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Năm sản xuất:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.manufacture_year }}
                  </div>
                </div>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Đợt nhập:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.import_batch }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Năm nhập:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.import_year }}
                  </div>
                </div>
              </div>
                <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Số CV,QĐ:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.dispatch_number }}
                  </div>
                </div>
              
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Pháp nhân:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.corporation_name }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Khu vực:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.device_area }}
                  </div>
                </div>
              </div>
              <div class="col-12 field p-0 pt-2">
                <span class="text-xl font-bold"> Thông tin thiết bị </span>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Tên thiết bị:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.device_name }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Đơn vị tính:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.device_unit_name }}
                  </div>
                </div>
              </div>
              <div class="col-12 p-0 field flex pb-3">
                <div class="w-8rem p-0">Quy cách:</div>
                <div
                  style="width: calc(100% - 8rem)"
                  class="p-0 pl-2 font-bold text-left"
                >
                  {{ deviceCard.device_des }}
                </div>
              </div>
              <div class="col-12 field p-0 flex align-items-center pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Loại thiết bị:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.device_type_name }}
                  </div>
                </div>
                <div class="col-6 p-0 flex align-items-center">
                  <div class="w-8rem p-0">Giá trị hiện tại:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                    v-if="deviceCard.current_price"
                  >
                    {{ deviceCard.current_price.toLocaleString() }} VND
                  </div>
                 
                </div>
              </div>

              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Thời gian BH:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.insurance_month }}
                    <span v-if="deviceCard.insurance_month">Tháng</span>
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Chu kỳ BH:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.insurance_cycle }}
                    <span v-if="deviceCard.insurance_cycle">Tháng</span>
                  </div>
                </div>
              </div>
              <div class="col-12 field p-0 flex pb-3">
                <div class="col-6 p-0 flex">
                  <div class="w-8rem p-0">Khấu hao:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                    v-if="deviceCard.depreciation_month"
                  >
                    {{ deviceCard.depreciation_month }} Tháng
                  </div>
                </div>
                <div
                  class="col-6 p-0 flex"
                  v-if="deviceCard.device_user_id == null"
                >
                  <div class="w-8rem p-0">Kho:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.warehouse_name }}
                  </div>
                </div>
                <div class="col-6 p-0 flex" v-else>
                  <div class="w-8rem p-0">Bộ phận quản lý:</div>
                  <div
                    style="width: calc(100% - 8rem)"
                    class="p-0 pl-2 font-bold text-left"
                  >
                    {{ deviceCard.manage_department_name }}
                  </div>
                </div>
              </div>
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-location-crosshairs"
                class="pr-2"
              />
              <span> Cấp phát sử dụng</span>
            </template>
            <div class="grid" v-if="listHandover.length > 0">
              <Accordion :multiple="true" :activeIndex="[0]" class="w-full">
                <AccordionTab
                  :header="'Số phiếu cấp phát: ' + item.handover_number"
                  v-for="(item, index) in listHandover"
                  :key="index"
                >
                  <div class="w-full">
                    <detailsHandover :handover="item" :check="1" />
                  </div>
                </AccordionTab>
              </Accordion>
            </div>
            <div v-else class="format-center font-bold font-italic">
              Tài sản này chưa được cấp phát sử dụng.
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-screwdriver-wrench"
                class="pr-2"
              />
              <span> Sửa chữa</span>
            </template>
            <div class="grid" v-if="listRepair.length > 0">
              <Accordion :multiple="true" :activeIndex="[0]" class="w-full">
                <AccordionTab
                  :header="'Số phiếu sửa chữa: ' + item.repair_number"
                  v-for="(item, index) in listRepair"
                  :key="index"
                >
                  <form>
                    <div class="grid formgrid m-2">
                      <div class="col-12 field p-0 text-lg font-bold">
                        Thông tin phiếu
                      </div>
                      <div class="col-12 field flex p-0">
                        <div class="col-6 flex p-0 align-items-center">
                          <div class="w-10rem">Số phiếu:</div>
                          <div
                            style="width: calc(100% - 10rem)"
                            class="font-bold"
                          >
                            {{ item.repair_number }}
                          </div>
                        </div>
                        <div
                          class="col-6 flex p-0 text-center align-items-center"
                        >
                          <div class="col-4 p-0 pl-5 text-left">Ngày lập:</div>
                          <div class="col-8 p-0 flex text-left font-bold">
                            {{
                              moment(new Date(item.repair_created_date)).format(
                                "DD/MM/YYYY"
                              )
                            }}
                          </div>
                        </div>
                      </div>

                      <div class="col-12 flex p-0">
                        <div class="col-6 p-0 text-left align-items-center">
                          <div
                            class="
                              col-12
                              field
                              p-0
                              flex
                              text-left
                              align-items-center
                            "
                          >
                            <div class="w-10rem">Kiểu phiếu:</div>
                            <div
                              style="width: calc(100% - 10rem)"
                              class="font-bold"
                            >
                              {{
                                item.repair_type == 1
                                  ? "Sửa chữa"
                                  : "Bảo trì - Bảo dưỡng"
                              }}
                            </div>
                          </div>
                        </div>
                        <div class="col-6 p-0 text-left align-items-center">
                          <div
                            class="
                              col-12
                              field
                              p-0
                              flex
                              text-left
                              align-items-center
                            "
                          >
                            <div class="col-4 p-0 pl-5 text-left">Nơi lập:</div>
                            <div
                              style="width: calc(100% - 10rem)"
                              class="font-bold"
                            >
                              {{ item.repair_created_place }}
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="field py-2 px-0 col-12 flex">
                        <div class="col-6 p-0 flex">
                          <div class="col-4 p-0 font-bold text-lg">
                            Người đề xuất
                          </div>
                        </div>
                      </div>
                      <div class="field p-0 col-12 flex">
                        <div class="col-6 p-0 flex align-items-center">
                          <div class="w-10rem">Người đề xuất:</div>
                          <div style="width: calc(100% - 10rem)">
                            <div class="country-item flex align-items-center">
                              <Avatar
                                v-bind:label="
                                  item.avartar
                                    ? ''
                                    : item.proposer.substring(
                                        item.proposer.lastIndexOf(' ') + 1,
                                        item.proposer.lastIndexOf(' ') + 2
                                      )
                                "
                                :image="basedomainURL + item.avartar"
                                class="w-2rem h-2rem"
                                size="large"
                                :style="
                                  item.avartar
                                    ? 'background-color: #2196f3'
                                    : 'background:' +
                                      bgColor[item.proposer.length % 7]
                                "
                                shape="circle"
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
                              />
                              <div class="pt-1 pl-2 font-bold">
                                {{ item.proposer }}
                              </div>
                            </div>
                          </div>
                        </div>
                        <div class="col-6 p-0 flex align-items-center">
                          <div class="col-4 p-0 pl-5">Phòng ban:</div>
                          <div class="col-8 p-0 font-bold">
                            {{ item.department_name }}
                          </div>
                        </div>
                      </div>
                      <div class="col-12 field flex">
                        <div class="col-6 p-0 flex"></div>
                        <div class="col-6 p-0 flex">
                          <div class="col-4 p-0 pl-5">Chức vụ:</div>
                          <div class="col-8 p-0 font-bold">
                            {{ item.position_name }}
                          </div>
                        </div>
                      </div>

                      <div class="field py-2 px-0 col-12 flex">
                        <div class="col-6 p-0 flex">
                          <div class="col-4 p-0 font-bold text-lg">
                            Thông tin thiết bị
                          </div>
                        </div>
                      </div>
                      <div class="col-12 field flex">
                        <div class="w-10rem">Tình trạng:</div>
                        <div
                          style="width: calc(100% - 10rem)"
                          class="font-bold"
                        >
                          {{ item.details_card.condition }}
                        </div>
                      </div>
                      <div class="col-12 field flex">
                        <div class="w-10rem">Phương hướng sửa:</div>
                        <div
                          style="width: calc(100% - 10rem)"
                          class="font-bold"
                        >
                          {{ item.details_card.repair_plan }}
                        </div>
                      </div>
                      <div class="col-12 field flex">
                        <div class="col-6 p-0 flex">
                          <div class="w-10rem">Tình trạng sửa:</div>
                          <div
                            style="width: calc(100% - 10rem)"
                            class="font-bold"
                          >
                            {{
                              item.details_card.repair_condition == "2"
                                ? "Trong kho chờ sửa chữa"
                                : item.details_card.repair_condition == "3"
                                ? "Hỏng không sửa được"
                                : item.details_card.repair_condition == "1"
                                ? "Hoàn thành sửa chữa"
                                : ""
                            }}
                          </div>
                        </div>
                        <div class="col-6 p-0 flex">
                          <div class="col-4 p-0 pl-5">Giá sửa chữa:</div>
                          <div class="col-8 p-0 font-bold">
                            {{
                              Number(
                                item.details_card.repair_price
                              ).toLocaleString()
                            }}
                            VND
                          </div>
                        </div>
                      </div>
                      <div class="col-12 field flex">
                        <div class="w-10rem">Ghi chú:</div>
                        <div
                          style="width: calc(100% - 10rem)"
                          class="font-bold"
                        >
                          {{ item.details_card.repair_note }}
                        </div>
                      </div>
                    </div>
                  </form>
                </AccordionTab>
              </Accordion>
            </div>
            <div v-else class="format-center font-bold font-italic">
              Tài sản này chưa được sửa chữa, bảo dưỡng.
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon icon="fa-solid fa-rotate-left" class="pr-2" />
              <span> Thu hồi</span>
            </template>
            <div class="grid" v-if="listRecall.length > 0">
              <Accordion :multiple="true" :activeIndex="[0]" class="w-full">
                <AccordionTab
                  :header="'Số phiếu thu hồi: ' + item.recall_number"
                  v-for="(item, index) in listRecall"
                  :key="index"
                >
                  <div class="w-full">
                    <detailsRecall
                      :device_recall_id="item.device_recall_id"
                      :check="1"
                    />
                  </div>
                </AccordionTab>
              </Accordion>
            </div>
            <div v-else class="format-center font-bold font-italic">
              Tài sản này chưa bị thu hồi.
            </div>
          </TabPanel>
          <!-- <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-money-bill-1-wave"
                class="pr-2"
              />
              <span> Thanh lý</span>
            </template>
            <p>Chưa có dữ liệu.</p>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon icon="fa-solid fa-square-minus" class="pr-2" />
              <span> Thất thoát</span>
            </template>
            <p>Chưa có dữ liệu.</p>
          </TabPanel> -->
          <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-boxes-stacked"
                class="pr-2"
              />
              <span> Kiểm kê</span>
            </template>
            <div class="grid" v-if="listInventory.length > 0">
              <Accordion :multiple="true" :activeIndex="[0]" class="w-full">
                <AccordionTab
                  :header="'Số phiếu kiểm kê: ' + item.inventory_number"
                  v-for="(item, index) in listInventory"
                  :key="index"
                >
                  <div class="w-full">
                    <detailsInventory
                      :device_inventory_id="item.inventory_slip_id"
                    />
                  </div>
                </AccordionTab>
              </Accordion>
            </div>
            <div v-else class="format-center font-bold font-italic">
              Tài sản này chưa được kiểm kê.
            </div>
          </TabPanel>
          <!-- <TabPanel>
            <template #header>
              <font-awesome-icon icon="fa-solid fa-cart-flatbed" class="pr-2" />
              <span> Điều chuyển</span>
            </template>
            <p>Chưa có dữ liệu.</p>
          </TabPanel> -->
        </TabView>
      </div>
    </div>
  </form>
</template>