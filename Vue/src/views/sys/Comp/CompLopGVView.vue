<script setup>
import { ref, defineProps, inject, onMounted, onUpdated } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
const props = defineProps({
  lophoc: Object,
  tudiens: Array,
  reloadComponnent: Function,
});
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
//Valid Form
const monhoc = ref({
  LopUser_ID: -1,
  Lophoc_ID: props.Lophoc_ID,
  STT: 1,
  IsType: 1,
  Trangthai: 1,
});
const monhoclops = ref([]);
const mlop = ref({});
const opAddMon = ref();
const tdTrangthais = [
  { value: 0, text: "Đã khoá" },
  { value: 1, text: "Bình thường" },
  { value: 2, text: "Đang đợi xác nhận" },
  { value: 3, text: "Kết thúc năm học" },
];
const tdTypes = [
  { value: 1, text: "Giảng dạy" },
  { value: 2, text: "Trợ giảng" },
  { value: 3, text: "Người quản lý" },
];
const submitted = ref(false);
//Khai báo biến
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const isFirst = ref(true);
const loadLopMonhoc = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "UserListByClass",
        par: [
          { par: "user_id ", va: store.getters.user.user_id },
          { par: "Lophoc_ID ", va: props.lophoc.Lophoc_ID },
          { par: "Namhoc_ID ", va: store.getters.namhoc.Namhoc_ID },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((item) => {
          if (item.user_id) {
            item.giaovien = {
              user_id: item.user_id,
              full_name: item.full_name,
              Avartar: item.Avartar,
            };
          }
        });
        monhoclops.value = data[0];
      } else {
        monhoclops.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      let sm = [...new Set(monhoclops.value.map((x) => x.user_id))].length;
      props.reloadComponnent({
        Lophoc_ID: props.lophoc.Lophoc_ID,
        Sogv: sm,
      });
    })
    .catch((error) => {});
};
const toggleAddmon = (event) => {
  opAddMon.value.toggle(event);
};
const addGVMon = (data) => {
  mlop.value = { Users: [{ user_id: data.user_id }], Bosach_ID: data.Bosach_ID };
  saveMon();
};
const saveMon = () => {
  opAddMon.value.hide();
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let mons = [];
  if (mlop.value.Users) {
    mlop.value.Users.forEach((m) => {
      mons.push({
        LopUser_ID: -1,
        Namhoc_ID: store.getters.namhoc.Namhoc_ID,
        Lophoc_ID: props.lophoc.Lophoc_ID,
        user_id: m.user_id,
        Bosach_ID: mlop.value.Bosach_ID,
        Trangthai: 1,
        IsType: mlop.value.IsType,
        STT: mons.length + 1,
        Monhoc_ID: mlop.value.Monhoc_ID,
      });
    });
  }
  axios
    .post(baseURL + "/api/Lophoc/Add_LophocUser", mons, config)
    .then((response) => {
      loadLopMonhoc();
      mlop.value.Khoihocs = [];
      swal.close();
    })
    .catch((error) => {
      swal.close();
    });
};
const saveTableMon = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let mons = [];
  if (monhoclops.value.length > 0) {
    monhoclops.value.forEach((m) => {
      mons.push({
        LopUser_ID: m.LopUser_ID,
        Lophoc_ID: props.lophoc.Lophoc_ID,
        Monhoc_ID: m.Monhoc_ID,
        Bosach_ID: m.Bosach_ID,
        IsType: m.IsType,
        Trangthai: m.Trangthai,
      });
    });
  }
  axios
    .put(baseURL + "/api/Lophoc/Update_LophocUser", mons, config)
    .then((response) => {
      loadLopMonhoc();
      mlop.value.Khoihocs = [];
      swal.close();
    })
    .catch((error) => {
      swal.close();
    });
};
const delMonhocLophoc = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá môn học này không!",
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
          .delete(baseURL + "/api/Lophoc/Del_LophocUser", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [md.LopUser_ID],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Môn học thành công!");
              loadLopMonhoc();
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
          });
      }
    });
};
onMounted(() => {
  loadLopMonhoc();
  return {};
});
onUpdated(() => {
  //console.log(props.lophoc);
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2'" style="min-height: 450px !important">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="monhoclops"
      dataKey="LopUser_ID"
      :showGridlines="true"
      :rowHover="true"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      rowGroupMode="subheader"
      groupRowsBy="full_name"
    >
      <template #groupheader="slotProps">
        <Avatar
          v-bind:label="
            slotProps.data.Avartar ? '' : slotProps.data.full_name.substring(0, 1)
          "
          v-bind:image="basedomainURL + slotProps.data.Avartar"
          style="background-color: #2196f3; color: #ffffff"
          class="mr-2"
          shape="circle"
        />
        {{ slotProps.data.full_name }}
        <Button
          v-if="props.lophoc.IsPermision"
          type="button"
          v-tooltip="'Thêm môn giảng dạy'"
          icon="pi pi-plus-circle"
          class="p-button-rounded p-button-secondary p-button-text"
          @click="addGVMon(slotProps.data)"
        ></Button>
      </template>
      <template #header>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Button
              v-if="props.lophoc.IsPermision"
              label="Cập nhật"
              icon="pi pi-save"
              @click="saveTableMon()"
            />
          </template>
          <template #end>
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="loadLopMonhoc()"
            />
            <Button
              v-if="props.lophoc.IsPermision"
              label="Thêm giáo viên"
              icon="pi pi-plus"
              class="mr-2"
              @click="toggleAddmon"
              aria:haspopup="true"
              aria-controls="overlay_panel"
            />
            <OverlayPanel
              ref="opAddMon"
              appendTo="body"
              :showCloseIcon="true"
              id="overlay_panel"
              style="width: 450px"
              :breakpoints="{ '960px': '75vw' }"
            >
              <div class="field col-12 md:col-12">
                <label class="col-3 text-left">Giáo viên</label>
                <MultiSelect
                  :virtualScrollerOptions="{ itemSize: 48 }"
                  :options="tudiens[3]"
                  v-model="mlop.Users"
                  optionLabel="full_name"
                  placeholder="Chọn giáo viên"
                  class="col-9"
                  :popup="true"
                  :filter="true"
                  :showClear="true"
                >
                  <template #value="slotProps">
                    <div
                      class="user-item user-item-value"
                      v-if="slotProps.value && slotProps.value.length > 0"
                      v-for="u in slotProps.value"
                      :key="u.user_id"
                    >
                      <Avatar
                        v-bind:label="u.Avartar ? '' : u.full_name.substring(0, 1)"
                        v-bind:image="basedomainURL + u.Avartar"
                        style="background-color: #2196f3; color: #ffffff"
                        class="mr-2"
                        shape="circle"
                      />
                      <div>{{ u.full_name }}</div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div class="user-item">
                      <Avatar
                        v-bind:label="
                          slotProps.option.Avartar
                            ? ''
                            : slotProps.option.full_name.substring(0, 1)
                        "
                        v-bind:image="basedomainURL + slotProps.option.Avartar"
                        style="background-color: #2196f3; color: #ffffff"
                        class="mr-2"
                        shape="circle"
                      />
                      <div>
                        {{ slotProps.option.full_name
                        }}<i class="gvpb">{{ slotProps.option.organization_name }}</i>
                      </div>
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-3">Loại</label>
                <Dropdown
                  v-model="mlop.IsType"
                  :options="tdTypes"
                  class="col-9"
                  optionLabel="text"
                  optionValue="value"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-3 text-left">Môn</label>
                <Dropdown
                  v-model="mlop.Monhoc_ID"
                  class="col-9"
                  :popup="true"
                  :filter="true"
                  :options="tudiens[4]"
                  optionLabel="text"
                  optionValue="value"
                  :showClear="true"
                  placeholder=""
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-3">Bộ sách</label>
                <Dropdown
                  class="col-9"
                  v-model="mlop.Bosach_ID"
                  :options="tudiens[5]"
                  optionLabel="text"
                  optionValue="value"
                  placeholder="Chọn bộ sách"
                  :showClear="true"
                />
              </div>
              <div
                class="field col-12 md:col-12 align-items-center justify-content-center text-center"
              >
                <Button
                  label="Cập nhật"
                  icon="pi pi-plus"
                  class="m-auto"
                  @click="saveMon"
                />
              </div>
            </OverlayPanel>
          </template>
        </Toolbar>
      </template>
      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column>
      <Column field="Monhoc_Ten" header="Môn học" class="align-items-center">
        <template #body="md">
          <Dropdown
            v-model="md.data.Monhoc_ID"
            class="w-full"
            :popup="true"
            :filter="true"
            :options="tudiens[4]"
            optionLabel="text"
            optionValue="value"
            :showClear="true"
            placeholder=""
          />
        </template>
      </Column>
      <Column
        field="TenBosach"
        header="Bộ sách"
        headerStyle="max-width:300px"
        bodyStyle="max-width:300px"
      >
        <template #body="md">
          <Dropdown
            v-model="md.data.Bosach_ID"
            :options="tudiens[5]"
            class="w-full"
            optionLabel="text"
            optionValue="value"
            :filter="true"
          />
        </template>
      </Column>
      <Column field="full_name" header="Giáo viên">
        <template #body="md">
          <Dropdown
            :virtualScrollerOptions="{ itemSize: 48 }"
            :options="tudiens[3]"
            v-model="md.data.giaovien"
            class="w-full"
            optionLabel="full_name"
            placeholder="Chọn giáo viên"
            :filter="true"
            :showClear="true"
          >
            <template #value="slotProps">
              <div
                class="user-item user-item-value"
                v-if="slotProps.value && slotProps.value.full_name"
              >
                <Avatar
                  v-bind:label="
                    slotProps.value.Avartar
                      ? ''
                      : slotProps.value.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.value.Avartar"
                  style="background-color: #2196f3; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ slotProps.value.full_name }}</div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div class="user-item">
                <Avatar
                  v-bind:label="
                    slotProps.option.Avartar
                      ? ''
                      : slotProps.option.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.Avartar"
                  style="background-color: #2196f3; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>
                  {{ slotProps.option.full_name
                  }}<i class="gvpb">{{ slotProps.option.organization_name }}</i>
                </div>
              </div>
            </template>
          </Dropdown>
        </template>
      </Column>
      <Column
        field="IsType"
        header="Loại GV"
        headerStyle="max-width:250px"
        bodyStyle="max-width:250px"
      >
        <template #body="md">
          <Dropdown
            v-model="md.data.IsType"
            :options="tdTypes"
            class="w-full"
            optionLabel="text"
            optionValue="value"
          />
        </template>
      </Column>
      <Column
        v-if="props.lophoc.IsPermision"
        headerClass="text-center"
        bodyClass="text-center"
        headerStyle="text-align:center;max-width:60px"
        bodyStyle="text-align:center;max-width:60px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delMonhocLophoc(md.data)"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="m-auto align-items-center justify-content-center p-4 text-center"
          v-if="!isFirst"
        >
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
</template>
<style scoped>
.gvpb {
  display: block;
  color: #607d8b;
  font-size: 13px;
}
</style>
