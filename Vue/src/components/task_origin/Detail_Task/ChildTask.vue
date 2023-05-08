<script setup>
import moment from "moment";
const basedomainURL = fileURL;
const props = defineProps({
  datalists: Object,
  addLinkTaskOrigin: Function,
  addNewChildTaskOrigin: Function,
  isClose: Boolean,
  ListChildTask: Array,
  bgColor: Array,
  show: Function,
});
</script>
<template>
  <div>
    <div class="row col-12">
      <div class="col-12 p-0 m-0">
        <div class="row col-12 p-0 m-0 font-bold text-xl">
          <div style="float: right">
            <ul
              id="task-child"
              style="display: flex; padding: 0px"
            >
              <li
                v-if="props.isClose == false"
                @click="props.addLinkTaskOrigin(props.datalists)"
                style="list-style: none; margin-right: 20px; color: #0d89ec"
              >
                <a style="display: flex; font-size: 12px"
                  ><i
                    style="margin-right: 5px"
                    class="p-custom pi pi-link"
                  ></i>
                  Liên kết công việc con</a
                >
              </li>
              <li
                v-if="props.isClose == false"
                @click="props.addNewChildTaskOrigin(props.datalists)"
                style="list-style: none; margin-right: 20px; color: #0d89ec"
              >
                <a style="display: flex; font-size: 12px"
                  ><i
                    style="margin-right: 5px"
                    class="p-custom pi pi-plus-circle"
                  ></i>
                  Tạo công việc con</a
                >
              </li>
            </ul>
          </div>
        </div>
      </div>
      <div
        class="col-12 p-0 m-0"
        style="height: 100%; overflow-y: auto"
      >
        <div
          v-if="props.ListChildTask && props.ListChildTask.length == 0"
          style="
            text-align: center;
            display: flex;
            flex-direction: column;
            align-items: center;
          "
          class="row col-12 p-0 m-0 pt-1 pb-1 pl-5"
        >
          <img
            style="width: 500px"
            v-bind:src="basedomainURL + '/Portals/Image/noproject.png'"
          />
          <span style="font-size: 20px; font-weight: bold; margin-top: 25px"
            >Hiện chưa có công việc con nào</span
          >
        </div>
        <div v-if="props.ListChildTask && props.ListChildTask.length > 0">
          <div
            class="row col-12 p-0 m-0 pt-1 pb-1 pl-5 child-task-hover"
            v-for="(ch, index) in props.ListChildTask"
            :key="index"
          >
            <div
              class="row col-12 flex p-0 m-0"
              @click="props.show(ch)"
            >
              <div class="col-7 p-0 m-0">
                <span class="font-bold text-xl">
                  {{ ch.task_name }}
                </span>
                <br />
                <span>
                  {{ moment(new Date(ch.start_date)).format("DD/MM/YYYY") }}
                </span>
                -
                <span v-if="ch.is_deadline == true">
                  {{ moment(new Date(ch.end_date)).format("DD/MM/YYYY") }}
                </span>
              </div>
              <div class="col-4 p-0 m-0 format-center">
                <AvatarGroup>
                  <div
                    v-for="(user, index) in ch.users"
                    :key="index"
                  >
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-if="user.is_type == 0"
                      v-tooltip.right="{
                        value: user.tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        user.avt
                          ? ''
                          : user.full_name.split(' ').at(-1).substring(0, 1)
                      "
                      v-bind:image="basedomainURL + user.avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: props.bgColor[index % 7],
                        border: '2px solid' + props.bgColor[index % 10],
                      }"
                      class=""
                      size="normal"
                      shape="circle"
                    />
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-if="user.is_type == 1"
                      v-tooltip.right="{
                        value: user.tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        user.avt
                          ? ''
                          : user.full_name.split(' ').at(-1).substring(0, 1)
                      "
                      v-bind:image="basedomainURL + user.avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: props.bgColor[index % 7],
                        border: '2px solid' + props.bgColor[index % 10],
                      }"
                      class=""
                      size="normal"
                      shape="circle"
                    />
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-if="user.is_type == 2"
                      v-tooltip.right="{
                        value: user.tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        user.avt
                          ? ''
                          : user.full_name.split(' ').at(-1).substring(0, 1)
                      "
                      v-bind:image="basedomainURL + user.avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: props.bgColor[index % 7],
                        border: '2px solid' + props.bgColor[index % 10],
                      }"
                      class=""
                      size="normal"
                      shape="circle"
                    />
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-if="user.is_type == 3"
                      v-tooltip.right="{
                        value: user.tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        user.avt
                          ? ''
                          : user.full_name.split(' ').at(-1).substring(0, 1)
                      "
                      v-bind:image="basedomainURL + user.avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: props.bgColor[index % 7],
                        border: '2px solid' + props.bgColor[index % 10],
                      }"
                      class=""
                      size="normal"
                      shape="circle"
                    />
                  </div>
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-if="ch.users.length > 4"
                    v-tooltip.right="{
                      value:
                        'và ' + (ch.users.length - 4) + ' người khác tham gia',
                    }"
                    :label="'+' + (ch.users.length - 1)"
                    style="color: #ffffff; cursor: pointer; font-size: 1rem"
                    :style="{
                      background: props.bgColor[index % 7],
                      border: '2px solid' + props.bgColor[index % 10],
                    }"
                    class=""
                    size="normal"
                    shape="circle"
                  ></Avatar>
                </AvatarGroup>
              </div>
              <div class="col-1 p-0 m-0 format-center">{{ ch.progress }}%</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@import "../style.scss";
</style>
