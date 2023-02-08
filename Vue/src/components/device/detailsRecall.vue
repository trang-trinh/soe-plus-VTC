
<script setup>
import { ref, inject, onMounted, watch } from "vue";
import moment from "moment";
import { encr } from "../../util/function.js";

const emitter = inject("emitter");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const props = defineProps({
  device_recall_id: Intl,
  check:Intl
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
const cryoptojs = inject("cryptojs");
const device_recall = ref();
const listAssetsH = ref([]);
const listFilesS = ref();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
const openDetailsHandover = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_recall_get",
            par: [{ par: "device_recall_id", va: props.device_recall_id }],
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
      device_recall.value = data[0];
      device_recall.value.recall_created_date = new Date(
        device_recall.value.recall_created_date
      );
      listAssetsH.value = data1;
      listFilesS.value = data2;
    })
    .catch((error) => {
      console.log(error);
    });
};
onMounted(() => {
  openDetailsHandover();
  return {};
});
</script>
<template>
  <form>
    <div class="grid formgrid m-2" v-if="device_recall">
      <div class="col-12 field p-0 text-lg font-bold">Thông tin phiếu</div>
      <div class="col-12 field flex p-0">
        <div class="col-6 flex p-0 align-items-center">
          <div class="w-10rem">Số phiếu:</div>
          <div style="width: calc(100% - 10rem)" class="font-bold">
       {{device_recall.recall_number}}
          </div>
        </div>
        <div class="col-6 flex p-0 text-center align-items-center">
          <div class="col-4 p-0 pl-5 text-left">Ngày lập:</div>
          <div class="col-8 p-0 flex text-left font-bold">
                 {{
                moment(new Date(device_recall.recall_created_date)).format("DD/MM/YYYY")
              }}
           
          </div>
        </div>
      </div>

      <div class="col-12 field flex p-0">
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Nơi lập:</div>
          <div style="width: calc(100% - 10rem)" class="font-bold">
            {{ device_recall.created_place }}
            <!-- <InputText v-model="device_recall.created_place"      :disabled="true"
              class="w-full class-disabled" /> -->
          </div>
        </div>
      </div>
      <div class="col-12 field flex p-0">
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Nội dung:</div>
          <div style="width: calc(100% - 10rem)"  class="font-bold">
            <div>
              {{ device_recall.content_recall }}
            </div>
            <!-- <Textarea
                autoResize
                v-model="device_recall.content_recall"
                class="w-full h-full  class-disabled"
                rows="2"
                cols="30"
                spellcheck="false"
         :disabled="true"
       
              ></Textarea> -->
          </div>
        </div>
      </div>

      <div class="col-12 field flex p-0">
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Lý do:</div>
          <div style="width: calc(100% - 10rem)"  class="font-bold">
            {{ device_recall.reason }}
            <!-- <Textarea
                autoResize
                v-model="device_recall.reason"
                class="w-full h-full class-disabled"
                rows="2"
                cols="30"
                spellcheck="false"
          :disabled="true"
             
           
              ></Textarea> -->
          </div>
        </div>
      </div>

      <div class="col-12 field p-0 text-lg font-bold">
        <Toolbar class="p-0 surface-0 border-none">
          <template #start>
            <div class="text-lg font-bold">Thông tin người thu hồi</div>
          </template>
          <template #end> </template>
        </Toolbar>
      </div>

      <div class="col-12 flex p-0 align-items-center">
        <div class="col-6 p-0 text-left align-items-center">
          <div class="col-12 field p-0 flex text-left align-items-center">
            <div class="w-10rem">Người thu hồi:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="flex p-2 align-items-center pr-2"
            >
              <Avatar
              
                v-bind:label="
                  device_recall.avatar
                    ? ''
                    : device_recall.full_name.substring(
                        device_recall.full_name.lastIndexOf(' ') + 1,
                        device_recall.full_name.lastIndexOf(' ') + 2
                      )
                "
                :image="basedomainURL + device_recall.avatar"
                size="small"
                :style="
                  device_recall.avatar
                    ? 'background-color: #2196f3'
                    : 'background:' +
                      bgColor[device_recall.full_name.length % 7]
                "
                shape="circle"
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
              />
              <div class="px-2 font-bold">
                {{ device_recall.full_name }}
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-5">Phòng ban:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="p-2 font-bold"
            >
              {{ device_recall.department_name }}
            </div>
          </div>
          
        </div>
      </div>
      <div class="col-12 flex p-0 align-items-center">
        <div class="col-6 p-0 text-left align-items-center">
          
        </div>
        <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-5">Chức vụ:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="p-2 font-bold"
            >
              {{ device_recall.position_name }}
            </div>
          </div>
          <div class="col-12 field p-0 flex text-left align-items-center">
            
          </div>
        </div>
      </div>
      <div class="col-12 field p-0 text-lg font-bold">
        <Toolbar class="p-0 surface-0 border-none">
          <template #start>
            <div class="text-lg font-bold">Người nhận</div>
          </template>
          <template #end> </template>
        </Toolbar>
      </div>

      <div class="col-12 flex p-0">
        <div class="col-6 p-0 text-left align-items-center">
          <div class="col-12   p-0 flex text-left align-items-center">
            <div class="w-10rem">Người nhận:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="flex p-2 align-items-center pr-2"
            >
              <Avatar
                v-bind:label="
                  device_recall.user_warehouse_avatar
                    ? ''
                    : device_recall.user_warehouse_name.substring(
                        device_recall.user_warehouse_name.lastIndexOf(' ') + 1,
                        device_recall.user_warehouse_name.lastIndexOf(' ') + 2
                      )
                "
                :image="basedomainURL + device_recall.user_warehouse_avatar"
                size="small"
                :style="
                  device_recall.user_warehouse_avatar
                    ? 'background-color: #2196f3'
                    : 'background:' +
                      bgColor[device_recall.user_warehouse_name.length % 7]
                "
                shape="circle"
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
              />
              <div class="px-2 font-bold">
                {{ device_recall.user_warehouse_name }} -
                {{ device_recall.warehouse_name }}
              </div>
            </div>
          </div>
        </div>
         <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-5">Phòng ban:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="p-2 font-bold"
            >
              {{ device_recall.user_warehouse_department }}
            </div>
          </div>
          
        </div>
      </div>
      <div class="col-12 flex p-0 align-items-center">
        <div class="col-6 p-0 text-left align-items-center">
          
        </div>
        <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12   p-0 flex text-left align-items-center">
            <div class="w-10rem pl-5">Chức vụ:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="p-2 font-bold"
            >
              {{ device_recall.user_warehouse_position }}
            </div>
          </div>
          <div class="col-12   p-0 flex text-left align-items-center">
            
          </div>
        </div>
      </div>

      <div class="col-12 field p-0">
        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Người sử dụng</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12   p-0 flex text-left align-items-center">
              <div class="w-10rem">Người sử dụng:</div>
              <div
                style="width: calc(100% - 10rem); border-radius: 5px"
                class="flex p-2 align-items-center pr-2"
              >
                <Avatar
                  v-bind:label="
                    device_recall.product_user_avatar
                      ? ''
                      : device_recall.product_user_name.substring(
                          device_recall.product_user_name.lastIndexOf(' ') + 1,
                          device_recall.product_user_name.lastIndexOf(' ') + 2
                        )
                  "
                  :image="basedomainURL + device_recall.product_user_avatar"
                  size="small"
                  :style="
                    device_recall.product_user_avatar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[device_recall.product_user_name.length % 7]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
                <div class="px-2 font-bold">
                  {{ device_recall.product_user_name }}
                </div>
              </div>
            </div>
          </div>
           <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-5">Phòng ban:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="p-2 font-bold"
            >
              {{ device_recall.product_user_department }}
            </div>
          </div>
          
        </div>
      </div>
      <div class="col-12 flex p-0 align-items-center">
        <div class="col-6 p-0 text-left align-items-center">
          
        </div>
        <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12   p-0 flex text-left align-items-center">
            <div class="w-10rem pl-5">Chức vụ:</div>
            <div
              style="width: calc(100% - 10rem); border-radius: 5px"
              class="p-2 font-bold"
            >
              {{ device_recall.product_user_position }}
            </div>
          </div>
          <div class="col-12   p-0 flex text-left align-items-center">
            
          </div>
        </div>
      </div>
      </div>
 <div class="col-12 field p-0 text-lg font-bold" v-if="check!=1">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Tài sản thu hồi</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>
      <div class="col-12 p-0"  v-if="check!=1">
        <div
          style="border-radius: 5px"
          class="
            w-full
            field
            p-2
            border-none
            image-container
            border-2 border-solid border-300
          "
          v-for="(item, index) in listAssetsH"
          :key="index"
        >
          <div class="product-item surface-50 p-0 w-full flex">
            <div class="w-10rem pr-2 relative">
              <Image
                :src="
                  item.image
                    ? basedomainURL + item.image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
                width="120"
                height="75"
                style="object-fit: cover; width: 100%"
                class="p-2 cursor-pointer"
                preview
                :alt="item.image"
                @error="
                  $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
            </div>
            <div class="product-list-detail w-full">
              <h5 class="my-2 text-justify text-lg pb-2">
                {{ item.device_name }}
              </h5>

              <div class="flex pb-2">
                <div class="w-full">
                  <i class="pi pi-tag product-category-icon"></i>
                  <span class="product-category pl-2">{{ item.device_number }}</span>
                </div>
                <div class="w-full">
                 <font-awesome-icon icon="fa-solid fa-barcode" />
                  <span class="product-category pl-2">{{ item.barcode_id }}</span>
                </div>
              </div>
              <div class="flex pb-2">
                <!-- <div class="w-full">
                  <i class="pi pi-home product-category-icon"></i>
                  <span class="product-category pl-2">
                    {{ item.warehouse_name }}
                  </span>
                </div> -->
                <div class="w-full">
                  <i class="pi pi-shopping-cart product-category-icon"></i>
                  <span class="product-category pl-2">
                    {{
                      moment(new Date(item.purchase_date)).format("DD/MM/YYYY")
                    }}
                  </span>
                </div>
              </div>
              <div class="product-item surface-50 p-0">
                <div class="product-list-detail">
                  <div class="flex pb-2">
                    <div class="w-full align-item-center">
                      <font-awesome-icon icon="fa-solid fa-money-bill-wave" />
                      <span class="product-category ">
                        Giá trị thiết bị:
                        {{ item.price ? item.price.toLocaleString() : 0 }} VND
                      </span>
                    </div>
                    <div class="w-full align-item-center">
                      <font-awesome-icon icon="fa-solid fa-money-bill-1-wave" />
                      <span class="product-category">
                        Giá trị hiện tại:
                        {{
                          item.current_price
                            ? item.current_price.toLocaleString()
                            : 0
                        }}
                        VND
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="pr-2">
              <!-- <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="deleteFileD(item)"
                /> -->
            </div>
          </div>

          <div class="w-full field flex align-items-center">
            <div class="w-10rem p-0 text-center font-bold">Tình trạng:</div>
            <div class="p-0" style="width: calc(100% - 10rem)">
              {{ item.condition }}
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 flex p-0 field mt-2 pt-2">
        <div v-if="listFilesS.length>0" class="w-10rem p-0 font-bold">File đính kèm:</div>
      </div>
      <div class="col-12 p-0">
        <div
          class="p-0 w-full px-8 field"
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
                    :alt="item.files_name"
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
                      class="w-full no-underline text-900"
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
                        :alt="item.files_name"
                      />
                    </a>
                  </div>
                  <a
                    :href="basedomainURL + item.file_path"
                    download
                    class="w-full no-underline text-900"
                  >
                    <span class="ml-2" style="line-height: 50px">
                      {{ item.files_name }}</span
                    >
                  </a>
                </div>
              </template>
           
            </Toolbar>
          </div>
        </div>
      </div>
    </div>
  </form>
</template>