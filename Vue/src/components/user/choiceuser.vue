<script setup>
import { arMA } from "date-fns/locale";
import { ref, inject, onMounted, watch } from "vue";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
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
//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  one: Boolean,
  datas: Array,
  values: String,
  keys: String,
  title: String,
  placeholder: String,
  removeuser: Function,
});
const temp = ref();
const changeuser = (value) => {
  if (value) {
    props.values = value[props.keys];
    console.log(props.values);
    debugger;
  }
};
</script>
<template>
  <div>
    <div v-if="props.one">
      <Dropdown
        :options="props.datas"
        :filter="true"
        :showClear="true"
        :editable="false"
        :optionValue="props.user_id"
        :optionLabel="props.title"
        :placeholder="props.placeholder"
        v-model="temp"
        class="ip36"
        @change="changeuser(temp)"
      >
        <template #value="slotProps">
          <div v-if="slotProps.value">
            <Chip
              :image="slotProps.value.avatar"
              :label="slotProps.value.full_name"
              class="mr-2 mb-2 pl-0"
            >
              <div class="flex">
                <div class="format-flex-center">
                  <Avatar
                    v-bind:label="
                      slotProps.value.avatar
                        ? ''
                        : (slotProps.value.last_name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.value.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 2rem;
                      height: 2rem;
                    "
                    :style="{
                      background: bgColor[slotProps.value.is_order % 7],
                    }"
                    class="mr-2 text-avatar"
                    size="xlarge"
                    shape="circle"
                  />
                </div>
                <div class="format-flex-center">
                  <span>{{ slotProps.value.full_name }}</span>
                </div>
                <span
                  tabindex="0"
                  class="
                    p-chip-remove-icon
                    pi pi-times-circle
                    format-flex-center
                  "
                  @click="props.remove(props.value)"
                ></span>
              </div>
            </Chip>
          </div>
          <span v-else> {{ slotProps.placeholder }} </span>
        </template>
        <template #option="slotProps">
          <div v-if="slotProps.option" class="flex">
            <div class="format-center">
              <Avatar
                v-bind:label="
                  slotProps.option.avatar
                    ? ''
                    : slotProps.option.last_name.substring(0, 1)
                "
                v-bind:image="basedomainURL + slotProps.option.avatar"
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 3rem;
                  height: 3rem;
                "
                :style="{
                  background: bgColor[slotProps.option.is_order % 7],
                }"
                class="mr-2 text-avatar"
                size="xlarge"
                shape="circle"
              />
            </div>
            <div class="ml-2">
              <div class="mb-1">{{ slotProps.option.full_name }}</div>
              <div class="description">
                <div>{{ slotProps.option.role_name }}</div>
                <div>{{ slotProps.option.department_name }}</div>
              </div>
            </div>
          </div>
          <span v-else> Chưa có dữ liệu tuần </span>
        </template>
      </Dropdown>
    </div>
    <div v-else>
      <MultiSelect
        :options="props.datachihuys"
        :filter="true"
        :showClear="true"
        :editable="false"
        optionLabel="full_name"
        placeholder="Chọn người chỉ huy"
        v-model="props.model.chihuys"
        class="ip36"
        style="height: auto; min-height: 36px"
      >
        <template #value="slotProps">
          <ul class="p-ulchip" v-if="slotProps.value">
            <li
              class="p-lichip"
              v-for="(value, user_id) in slotProps.value"
              :key="user_id"
            >
              <Chip
                :image="value.avatar"
                :label="value.full_name"
                class="mr-2 mb-2 pl-0"
              >
                <div class="flex">
                  <div class="format-flex-center">
                    <Avatar
                      v-bind:label="
                        value.avatar
                          ? ''
                          : (value.last_name ?? '').substring(0, 1)
                      "
                      v-bind:image="basedomainURL + value.avatar"
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 2rem;
                        height: 2rem;
                      "
                      :style="{
                        background: bgColor[value.is_order % 7],
                      }"
                      class="mr-2 text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="format-flex-center">
                    <span>{{ value.full_name }}</span>
                  </div>
                </div>
              </Chip>
            </li>
          </ul>
          <span v-else> {{ slotProps.placeholder }} </span>
        </template>
        <template #option="slotProps">
          <div v-if="slotProps.option" class="flex">
            <div class="format-center">
              <Avatar
                v-bind:label="
                  slotProps.option.avatar
                    ? ''
                    : slotProps.option.last_name.substring(0, 1)
                "
                v-bind:image="basedomainURL + slotProps.option.avatar"
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 3rem;
                  height: 3rem;
                "
                :style="{
                  background: bgColor[slotProps.option.is_order % 7],
                }"
                class="text-avatar"
                size="xlarge"
                shape="circle"
              />
            </div>
            <div class="ml-3">
              <div class="mb-1">{{ slotProps.option.full_name }}</div>
              <div class="description">
                <div>{{ slotProps.option.role_name }}</div>
                <div>{{ slotProps.option.department_name }}</div>
              </div>
            </div>
          </div>
          <span v-else> Chưa có dữ liệu tuần </span>
        </template>
      </MultiSelect>
    </div>
  </div>
</template>
<style scoped>
.ip36 {
  width: 100%;
}
.description {
  color: #aaa;
  font-size: 12px;
}
</style>