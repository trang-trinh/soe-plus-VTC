
<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
 
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
 
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
 
const props = defineProps({
  device_inventory_id: Intl,
});
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
const device_inventory=ref();
const loadData = () => {
  listUserA.value = [];

  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_accept_inventory_get",
            par: [{ par: "inventory_slip_id", va: props.device_inventory_id }],
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
      let data3 = JSON.parse(response.data.data)[3];

      device_inventory.value = data[0];

      device_inventory.value.inventory_date = new Date(
        device_inventory.value.inventory_date
      );
      if (device_inventory.value.inventory_created_date) {
        device_inventory.value.inventory_created_date = new Date(
          device_inventory.value.inventory_created_date
        );
      }

      data1.forEach((element, i) => {
        element.is_order = i + 1;
        if (element.representative) {
          element.representative = JSON.parse(element.representative)[0];
        }
      });

      selectedCard.value = data1;
      listFilesS.value = data2;
      data3.forEach((element, i) => {
        element.is_order = i + 1;
      });
      listUserA.value = data3;
       isShow.value=true;
    })
    .catch((error) => {
      console.log(error);
    });
};
const selectedCard = ref([]);
  const listFilesS=ref([]);
const listUserA = ref([]);
 
const isShow = ref(false);
onMounted(() => {
  loadData();
  return {};
});
</script>
<template>
  <form v-if="isShow">
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin phiếu</div>
        <div class="col-12 field flex p-0">
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem">Số phiếu:</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                :disabled="true"
                v-model="device_inventory.inventory_number"
                class="w-full class-disabled"
              />
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-4 p-0 pl-5 text-left">Người lập:</div>
            <div class="col-8 p-0 flex text-left font-bold">
              {{ device_inventory.created_name }}
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Ngày lập:</div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  :disabled="true"
                  placeholder="dd/mm/yyyy"
                  class="w-full class-disabled"
                  id="basic_use_date"
                  v-model="device_inventory.inventory_date"
                  autocomplete="on"
                  :showIcon="true"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5 text-left">Ngày kiểm kê:</div>
              <Calendar
                placeholder="dd/mm/yyyy"
                class="w-full class-disabled"
                id="basic_use_date"
                v-model="device_inventory.inventory_created_date"
                autocomplete="on"
                :disabled="true"
                :showIcon="true"
              />
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center" >
              <div class="w-10rem">Phòng ban:</div>
              <div style="width: calc(100% - 10rem)">
          
                <InputText
                  :disabled="true"
                  v-model="device_inventory.device_fromname"
                  class="w-full class-disabled"
                />
               
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center"   v-if="device_inventory.warehouse_id">
              <div class="col-4 p-0 pl-5 text-left">Kho:</div>

              <InputText
                v-if="device_inventory.warehouse_id"
                :disabled="true"
                v-model="device_inventory.device_fromname"
                class="w-full class-disabled"
              />
              <InputText
                v-else
                :disabled="true"
                v-model="device_inventory.department_name"
                class="w-full class-disabled"
                placeholder="Tài sản thuộc phòng ban"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Danh sách người tham gia</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 p-0 field">
          <div class="w-full p-0">
            <DataTable
              class="w-full p-datatable-sm e-sm"
              filterDisplay="menu"
              filterMode="lenient"
              dataKey="card_id"
              responsiveLayout="scroll"
              :scrollable="true"
              scrollHeight="flex"
              :showGridlines="true"
              :lazy="true"
              :value="listUserA"
              :paginator="false"
              :totalRecords="listUserA.length"
              :row-hover="true"
            >
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:70px;height:50px"
                bodyStyle="text-align:center;max-width:70px;overflow: hidden;"
                field="is_order"
                header="STT"
                headerClass="format-center"
              >
              </Column>
              <Column
                headerStyle="text-align:left;height:50px"
                bodyStyle="text-align:left;overflow: hidden;"
                headerClass="format-center"
                field="full_name"
                header="Họ và tên"
              >
                <template #body="data">
                  <div>
                    <div class="flex w-full align-items-center pr-2">
                      <Avatar
                        v-bind:label="
                          data.data.avatar
                            ? ''
                            : data.data.full_name.substring(
                                data.data.full_name.lastIndexOf(' ') + 1,
                                data.data.full_name.lastIndexOf(' ') + 2
                              )
                        "
                        :image="basedomainURL + data.data.avatar"
                        size="small"
                        :style="
                          data.data.avatar
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[data.data.full_name.length % 7]
                        "
                        shape="circle"
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                      />
                      <div class="px-2">{{ data.data.full_name }}</div>
                    </div>
                  </div>
                </template>
              </Column>

              <Column
                headerStyle="text-align:left;max-width:300px;height:50px"
                bodyStyle="text-align:left;max-width:300px; overflow: hidden;"
                field="organization_name"
                header="Phòng ban"
                headerClass="format-center"
              >
              </Column>

              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:220px;height:50px"
                bodyStyle="text-align:center;max-width:220px;overflow: hidden;"
                field="position_name"
                header="Chức vụ"
              >
              </Column>

              <template #empty>
                <div
                  class="align-items-center justify-c p-0 text-center m-auto"
                  v-if="!isFirst"
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
          </div>
        </div>
        <div class="col-12 pb-2 p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Danh sách thiết bị </div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 p-0 field">
          <div class="w-full">
            <DataTable
              class="w-full p-datatable-sm e-sm p-tbl-cs"
              filterDisplay="menu"
              filterMode="lenient"
              dataKey="card_id"
              responsiveLayout="scroll"
              scrollHeight="flex"
              :showGridlines="true"
              :lazy="true"
              :value="selectedCard"
              :paginator="false"
              :totalRecords="selectedCard.length"
              sortMode="single"
              rowGroupMode="rowspan"
              groupRowsBy="representative.device_number"
            >
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;width:50px;"
                bodyStyle="text-align:center;width:50px;overflow: hidden;"
                header="STT"
              >
                <template #body="data">
                  <div>
                    {{ data.index + 1 }}
                  </div>
                </template>
              </Column>

              <Column
                headerStyle="text-align:left;width:120px"
                bodyStyle="text-align:left;width:120px;overflow: hidden; "
                field="representative.device_number"
                header="Số hiệu"
              >
                <template #body="slotProps">
                  <div
                    class="image-text format-center"
                    v-if="slotProps.data.representative"
                  >
                    {{ slotProps.data.representative.device_number }}
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:150px"
                bodyStyle="text-align:left;overflow: hidden;width:150px"
                field="representative.device_number"
                header="Tên thiết bị"
              >
                <template #body="slotProps">
                  <span
                    class="image-text"
                    v-if="slotProps.data.representative"
                    >{{ slotProps.data.representative.device_name }}</span
                  >
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:120px"
                bodyStyle="text-align:left;width:120px;overflow: hidden;"
                field="representative.device_number"
                header="Sử dụng"
              >
                <template #body="slotProps">
                  <div class="w-full">
                    <span
                      class="image-text"
                      v-if="slotProps.data.representative"
                      >{{ slotProps.data.representative.full_name }}</span
                    >
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:70px;"
                bodyStyle="text-align:left;width:70px;overflow: hidden;"
                field="representative.device_number"
                header="SL trước"
              >
                <template #body="slotProps">
                  <div
                    class="image-text format-center"
                    v-if="slotProps.data.representative"
                  >
                    <Button
                      class="p-button-rounded"
                      :label="slotProps.data.representative.amount_before"
                    ></Button>
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:70px;"
                bodyStyle="text-align:left;width:70px;overflow: hidden;"
                field="representative.device_number"
                header="SL sau"
              >
                <template #body="slotProps">
                  <div
                    class="image-text format-center"
                    v-if="slotProps.data.representative"
                  >
                    <Button
                      class="p-button-rounded"
                      :label="slotProps.data.representative.amount_after"
                    ></Button>
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:150px;"
                bodyStyle="text-align:left;width:150px;overflow: hidden;"
                field="representative.device_number"
                header="Tình trạng"
              >
                <template #body="slotProps">
                  <span
                    class="image-text"
                    v-if="slotProps.data.representative"
                    >{{ slotProps.data.representative.condition }}</span
                  >
                </template>
              </Column>
              <Column
                headerStyle="text-align:left; min-width:120px"
                bodyStyle="text-align:left; overflow: hidden; min-width:120px"
                field="full_name"
                header="Người đánh giá"
              >
                <template #body="data">
                  <div class="flex w-full align-items-center pr-2">
                    <Avatar
                      v-bind:label="
                        data.data.avatar
                          ? ''
                          : data.data.full_name.substring(
                              data.data.full_name.lastIndexOf(' ') + 1,
                              data.data.full_name.lastIndexOf(' ') + 2
                            )
                      "
                      :image="basedomainURL + data.data.avatar"
                      size="small"
                      style="min-width: 25px"
                      :style="
                        data.data.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' +
                            bgColor[data.data.full_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                    />
                    <div class="px-2">{{ data.data.full_name }}</div>
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:70px;height:50px"
                bodyStyle="text-align:left;width:70px;overflow: hidden;"
                field="representative.amount"
                header="Số lượng"
              >
                <template #body="slotProps">
                  <div
                    class="w-full format-center"
                    v-if="slotProps.data.is_approved == false"
                  >
                    <Button
                      v-tooltip.top="'Chưa đánh giá'"
                      icon="pi pi-times"
                      class="p-button-rounded p-button-danger"
                    />
                  </div>
                  <div v-else class="format-center">
                    <Button
                      class="p-button-rounded"
                      :label="
                        slotProps.data.amount ? slotProps.data.amount : '0'
                      "
                    ></Button>
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;height:50px; min-width:120px"
                bodyStyle="text-align:left;overflow: hidden; min-width:120px"
                field="representative.reviews"
                header="Đánh giá"
              >
                <template #body="slotProps">
                  <div
                    class="w-full format-center"
                    v-if="slotProps.data.is_approved == false"
                  >
                    <Button
                      v-tooltip.top="'Chưa đánh giá'"
                      icon="pi pi-times"
                      class="p-button-rounded p-button-danger"
                    />
                  </div>
                  <div class="w-full" v-else>
                    {{ slotProps.data.reviews }}
                  </div>
                </template>
              </Column>

              <template #empty>
                <div
                  class="align-items-center justify-c p-0 text-center m-auto"
                  v-if="!isFirst"
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
          </div>
        </div>

        <div
          class="col-12 flex p-0 field mt-2 pt-2"
          v-if="listFilesS.length > 0"
        >
          <div class="w-10rem p-0 font-bold">File đính kèm</div>
        </div>
        <div class="col-12 p-0">
          <div class="w-10rem p-0"></div>
          <div
            style="width: calc(100%)"
            class="p-0 field px-8"
            v-for="(item, index) in listFilesS"
            :key="index"
          >
            <div
              class="
                p-0
                border-3 border-solid border-round-3xl border-blue-200
                surface-50
              "
              style="width: 100%; border-radius: 10px"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <Image
                      v-if="checkImg(item.file_path)"
                      :src="basedomainURL + item.file_path"
                      :alt="item.file_name"
                      width="70"
                      height="50"
                      style="
                        object-fit: contain;
                        border: 1px solid #ccc;
                        width: 70px;
                        height: 50px;
                      "
                      preview
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="pr-2"
                    />
                    <div v-else>
                      <a
                        :href="basedomainURL + item.file_path"
                        download
                        class="w-full no-underline"
                      >
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.file_path.substring(
                              item.file_path.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="width: 70px; height: 50px; object-fit: contain"
                          :alt="item.file_name"
                        />
                      </a>
                    </div>
                  <a
                    :href="basedomainURL + item.file_path"
                    download
                    class="w-full no-underline text-900"
                  >
                    <span class="ml-2" style="line-height: 50px">
                      {{ item.file_name }}</span
                    >
                  </a>
                  </div>
                </template>
                <template #end>
                  <!-- <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileH(item)"
                  /> -->
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
      </div>
  </form>
</template>