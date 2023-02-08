<script setup>
import { ref, defineProps, inject, onMounted, onUpdated } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
const emitter = inject("emitter");
const props = defineProps({
  displayAddCongViec: Boolean,
  Duan_ID: String,
  Muctieu_ID: String,
  Congviec_ID: String,
});
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
//Valid Form
const congviec = ref({
  Congviec_ID: "",
  Duan_ID: props.Duan_ID,
  Muctieu_ID: props.Muctieu_ID,
  Uutien: 1,
  Mucdo: 1,
  Congviec_Ten: "",
  STT: 1,
  Trangthai: 1,
});
const tdTrangthais = ref([
  { value: 0, text: "Đang lập kế hoạch" },
  { value: 1, text: "Đang thực hiện" },
  { value: 2, text: "Đã hoàn thành" },
  { value: 3, text: "Chờ xác nhận" },
  { value: 4, text: "Hoàn thành sau hạn" },
  { value: 5, text: "Tạm dừng" },
  { value: 6, text: "Đóng" },
]);
const submitted = ref(false);
const rules = {
  Congviec_Ten: {
    required,
  },
};
const v$ = useVuelidate(rules, congviec);
//Khai báo biến
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const isAdd = ref(true);
const modeldate = ref({});
const toast = useToast();
const tdUsers = ref([]);
const tdTukhoas = ref([]);
const tdMucdos = ref([]);
const tdUutiens = ref([]);
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Congviec_ListTudien",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Duan_ID", va: congviec.value.Duan_ID },
          { par: "Muctieu_ID", va: congviec.value.Muctieu_ID },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tdUsers.value = data[0];
        tdTukhoas.value = data[1];
        tdMucdos.value = data[2];
        tdUutiens.value = data[3];
        modeldate.value.thuchiens = data[0]
          .filter((x) => x.user_id == store.getters.user.user_id)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
      }
    })
    .catch(() => {});
};
const editCongviec = (Congviec_ID) => {
  submitted.value = false;
  isAdd.value = false;
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
        proc: "Plan_Congviec_Get",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Congviec_ID", va: Congviec_ID },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Tukhoa) obj.Tukhoa = obj.Tukhoa.split(",");
        modeldate.value.thoigiankehoach = [];
        if (obj.NgayBD) {
          modeldate.value.thoigiankehoach.push(new Date(obj.NgayBD));
        }
        if (obj.NgayKT) {
          modeldate.value.thoigiankehoach.push(new Date(obj.NgayKT));
        }
        if (modeldate.value.thoigiankehoach.length == 0) {
          modeldate.value.thoigiankehoach = null;
        }
        congviec.value = obj;
        //
        modeldate.value.thuchiens = data[1]
          .filter((x) => x.TypeUser == 1)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
        modeldate.value.quanlys = data[1]
          .filter((x) => x.TypeUser == 2)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
        modeldate.value.theodoi = data[1]
          .filter((x) => x.TypeUser == 0)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  addCongviec();
};
const addCongviec = () => {
  congviec.value.Muctieu_ID = props.Muctieu_ID;
  congviec.value.Duan_ID = props.Duan_ID;
  let data = {};
  let md = { ...congviec.value };
  if (md.Tukhoa instanceof Array) {
    md.Tukhoa = md.Tukhoa.join(",");
  }
  if (modeldate.value.thoigiankehoach) {
    if (modeldate.value.thoigiankehoach.length > 0) {
      md.NgayBD = modeldate.value.thoigiankehoach[0];
    }
    if (modeldate.value.thoigiankehoach.length > 1) {
      md.NgayKT = modeldate.value.thoigiankehoach[1];
    }
  }
  let users = [];
  if (modeldate.value.thuchiens) {
    modeldate.value.thuchiens.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 1, Trangthai: true, IsCheck: false });
    });
  }
  if (modeldate.value.quanlys) {
    modeldate.value.quanlys.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 2, Trangthai: true, IsCheck: false });
    });
  }
  if (modeldate.value.theodoi) {
    modeldate.value.theodoi.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 0, Trangthai: true, IsCheck: false });
    });
  }
  if (users.length > 0) {
    //formData.append("users", JSON.stringify(users));
    data.users = users;
  }
  data.model = md;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/Congviec/${isAdd.value == false ? "Update_Congviec" : "Add_Congviec"}`,
    data: data,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật công việc thành công!");
        closedisplayAdd();
        emitter.emit("duan", { type: "loadcongviec" });
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const removeUser = (type, uid) => {
  let idx = -1;
  switch (type) {
    case 1:
      idx = modeldate.value.thuchiens.findIndex((x) => x.user_id == uid);
      if (idx != -1) {
        modeldate.value.thuchiens.splice(idx, 1);
      }
      break;
    case 2:
      idx = modeldate.value.quanlys.findIndex((x) => x.user_id == uid);
      if (idx != -1) {
        modeldate.value.quanlys.splice(idx, 1);
      }
      break;
    case 0:
      idx = modeldate.value.theodoi.findIndex((x) => x.user_id == uid);
      if (idx != -1) {
        modeldate.value.theodoi.splice(idx, 1);
      }
      break;
  }
};
const closedisplayAdd = () => {
  emitter.emit("duan", { type: "closedisplayAddCongviec" });
};
onMounted(() => {
  initTudien();
  return {};
});
onUpdated(() => {
  if (props.displayAddCongViec) {
    if (props.Congviec_ID) {
      editCongviec(props.Congviec_ID);
    } else {
      isAdd.value = true;
      congviec.value = {
        Congviec_ID: "",
        Duan_ID: props.Duan_ID,
        Muctieu_ID: props.Muctieu_ID,
        Uutien: 1,
        Mucdo: 1,
        Congviec_Ten: "",
        STT: 1,
        Trangthai: 1,
      };
    }
  }
});
</script>
<template>
  <Dialog
    header="Cập nhật công việc"
    v-model:visible="displayAddCongViec"
    :style="{ width: '960px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
    :closable="false"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mã </label>
          <InputText
            spellcheck="false"
            placeholder="Có thể để trống"
            v-bind:disabled="!isAdd"
            class="col-4 ip36"
            v-model="congviec.Congviec_ID"
          />
          <label class="col-2 text-right">Thời gian từ </label>
          <Calendar
            class="col-4 ml-0 p-0"
            id="thoigiankehoach"
            v-model="modeldate.thoigiankehoach"
            selectionMode="range"
            :showIcon="true"
            :manualInput="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="congviec.Congviec_Ten"
            :class="{ 'p-invalid': v$.Congviec_Ten.$invalid && submitted }"
          />
        </div>
        <small
          v-if="
            (v$.Congviec_Ten.$invalid && submitted) || v$.Congviec_Ten.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Congviec_Ten.required.$message
                .replace("Value", "Tên mục tiêu")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Từ khoá</label>
          <Chips
            spellcheck="false"
            class="col-10 p-0"
            v-model="congviec.Tukhoa"
            :addOnBlur="true"
            separator=","
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Ưu tiên</label>
          <Dropdown
            class="col-4"
            v-model="congviec.Uutien"
            :options="tdUutiens"
            optionLabel="Ten"
            optionValue="Uutien"
            placeholder="Chọn ưu tiên"
          >
            <template #option="slotProps">
              <div class="p-dropdown-car-option">
                <Chip
                  :style="{
                    background: slotProps.option.Maunen,
                    color: slotProps.option.Mauchu,
                  }"
                  v-bind:label="slotProps.option.Ten"
                  class="mr-2 mb-2"
                />
              </div>
            </template>
          </Dropdown>

          <label class="col-2 text-right">Mức độ</label>
          <Dropdown
            class="col-4"
            v-model="congviec.Mucdo"
            :options="tdMucdos"
            optionLabel="Ten"
            optionValue="Mucdo"
            placeholder="Chọn mức độ"
          >
            <template #option="slotProps">
              <div class="p-dropdown-car-option">
                <Chip
                  :style="{
                    background: slotProps.option.Maunen,
                    color: slotProps.option.Mauchu,
                  }"
                  v-bind:label="slotProps.option.Ten"
                  class="mr-2 mb-2"
                />
              </div>
            </template>
          </Dropdown>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-4 ip36 p-0" v-model="congviec.STT" />

          <label class="col-2 text-right">Trạng thái</label>
          <Dropdown
            class="col-4"
            v-model="congviec.Trangthai"
            :options="tdTrangthais"
            optionLabel="text"
            optionValue="value"
            placeholder="Chọn trạng thái"
          />
        </div>
        <Accordion class="w-full m-1">
          <AccordionTab header="Thông tin thêm">
            <div class="field col-12 md:col-12">
              <label class="col-2 text-left">Người thực hiện</label>
              <MultiSelect
                v-model="modeldate.thuchiens"
                :virtualScrollerOptions="{ itemSize: 10 }"
                :options="tdUsers"
                optionLabel="full_name"
                placeholder="Chọn người thực hiện"
                :filter="true"
                class="col-10 multiselect-thuchien"
              >
                <template #value="slotProps">
                  <div
                    class="user-item user-item-value"
                    v-for="option of slotProps.value"
                    :key="option.user_id"
                  >
                    <Avatar
                      v-bind:label="option.Avartar ? '' : option.full_name.substring(0, 1)"
                      v-bind:image="basedomainURL + option.Avartar"
                      style="background-color: #2196f3; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ option.full_name }}</div>
                    <Button
                      @click="removeUser(1, option.user_id)"
                      icon="pi pi-times-circle"
                      class="hover-none p-button-text p-button-plain"
                      style="color: #fff"
                    />
                  </div>
                  <template v-if="!slotProps.value || slotProps.value.length === 0">
                    Chọn người thực hiện
                  </template>
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
                    <div>{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-2 text-left">Người quản lý</label>
              <MultiSelect
                v-model="modeldate.quanlys"
                :virtualScrollerOptions="{ itemSize: 10 }"
                :options="tdUsers"
                optionLabel="full_name"
                placeholder="Chọn người quản lý"
                :filter="true"
                class="col-10 multiselect-quanly"
              >
                <template #value="slotProps">
                  <div
                    class="user-item user-item-value"
                    v-for="option of slotProps.value"
                    :key="option.user_id"
                  >
                    <Avatar
                      v-bind:label="option.Avartar ? '' : option.full_name.substring(0, 1)"
                      v-bind:image="basedomainURL + option.Avartar"
                      style="background-color: #22c55e; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ option.full_name }}</div>
                    <Button
                      @click="removeUser(2, option.user_id)"
                      icon="pi pi-times-circle"
                      class="hover-none p-button-text p-button-plain"
                      style="color: #fff"
                    />
                  </div>
                  <template v-if="!slotProps.value || slotProps.value.length === 0">
                    Chọn người quản lý
                  </template>
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
                      style="background-color: #22c55e; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-2 text-left">Người Theo dõi</label>
              <MultiSelect
                v-model="modeldate.theodoi"
                :virtualScrollerOptions="{ itemSize: 10 }"
                :options="tdUsers"
                optionLabel="full_name"
                placeholder="Chọn người theo dõi"
                :filter="true"
                class="col-10 multiselect-theodoi"
              >
                <template #value="slotProps">
                  <div
                    class="user-item user-item-value"
                    v-for="option of slotProps.value"
                    :key="option.user_id"
                  >
                    <Avatar
                      v-bind:label="option.Avartar ? '' : option.full_name.substring(0, 1)"
                      v-bind:image="basedomainURL + option.Avartar"
                      style="background-color: #64748b; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ option.full_name }}</div>
                    <Button
                      @click="removeUser(0, option.user_id)"
                      icon="pi pi-times-circle"
                      class="hover-none p-button-text p-button-plain"
                      style="color: #fff"
                    />
                  </div>
                  <template v-if="!slotProps.value || slotProps.value.length === 0">
                    Chọn người theo dõi
                  </template>
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
                      style="background-color: #64748b; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div class="col-12">
              <label class="col-12 text-rigth">Mô tả</label>
              <div class="p-2">
                <Editor
                  v-model="congviec.Mota"
                  spellcheck="false"
                  editorStyle="height: 100px;font-size:14px"
                />
              </div>
            </div>
            <div class="col-12">
              <label class="col-12 text-rigth">Khó khăn</label>
              <div class="p-2">
                <Editor
                  v-model="congviec.Khokhan"
                  spellcheck="false"
                  editorStyle="height: 100px;font-size:14px"
                />
              </div>
            </div>
          </AccordionTab>
        </Accordion>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAdd"
        class="p-button-raised p-button-secondary"
      />
      <Button
        label="Cập nhật"
        icon="pi pi-save"
        @click="handleSubmit(!v$.$invalid)"
        
      />
    </template>
  </Dialog>
</template>
<style scoped></style>
