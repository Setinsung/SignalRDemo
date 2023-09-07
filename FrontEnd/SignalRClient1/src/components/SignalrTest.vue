<script setup lang="ts">
import { ref } from 'vue';
import * as signalR from '@microsoft/signalr';
const userMessage = ref("");
const messages = ref<string[]>([]);

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5092/Hubs/ChatRoomHub",  // 指定路径，设置禁用协商
    { skipNegotiation: true, transport: signalR.HttpTransportType.WebSockets }
  )
  .withAutomaticReconnect()
  .build();
await connection.start();
connection.on("ReceivePublicMessage", (msg: string) => { // 监听服务端发送的消息：后端的SendAsync方法发送的ReceivePublicMessage有多少参数这里就需要写多少个参数
  messages.value.push(msg);
});

const txtMsg = async () => {
  await connection.invoke("SendPublicMessage", userMessage.value); // 客户端向服务器端发送消息：invoke方法指定后端Hub中定义的方法名传输数据
  userMessage.value = "";
};
</script>

<template>
  <h2>SignalRClient1</h2>
  <input type="text" v-model="userMessage" @keyup.enter="txtMsg" />
  <div>
    <ul>
      <li v-for="(msg, index) in messages" :key="index">{{ msg }}</li>
    </ul>
  </div>
</template>