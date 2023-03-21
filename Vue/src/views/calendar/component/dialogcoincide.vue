<script setup>
import { ref, inject, onMounted } from "vue";
const basedomainURL = baseURL;
//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  datas: Array,
  goCalendar: Function,
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 1003"
  >
    <form>
      <div class="row m-2">
        <div class="col-12 md:col-12">
          <div>
            <DataTable
              :value="datas"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              :scrollable="true"
              scrollHeight="flex"
              filterDisplay="menu"
              filterMode="lenient"
            >
              <Column
                field="coincide"
                header="Lịch trùng"
                headerStyle="text-align:center;max-width:250px;height:50px;"
                bodyStyle="max-width:250px;max-height:60px"
              >
              </Column>
              <Column
                field="contents"
                header="Nội dung trùng"
                headerStyle="height:50px;max-width:auto;min-width:150px;"
                bodyStyle="max-height:60px;"
              >
                <template #body="slotProps">
                  <div v-html="slotProps.data.contents"></div>
                </template>
              </Column>
              <Column
                header="Chức năng"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;max-height:60px"
              >
                <template #body="slotProps">
                  <div>
                    <Button
                      @click="props.goCalendar(slotProps.data)"
                      class="
                        p-button-rounded p-button-secondary p-button-outlined
                        mx-1
                        mb-2
                      "
                      type="button"
                      icon="pi pi-eye"
                      v-tooltip.top="'Xem'"
                    ></Button>
                  </div>
                </template>
              </Column>
              <template #empty>
                <div
                  v-if="datas.length == 0"
                  class="
                    flex
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                    m-auto
                  "
                >
                  <div>
                    <img
                      src="../../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </div>
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
    </template>
  </Dialog>
</template>
<style scoped>
</style>
