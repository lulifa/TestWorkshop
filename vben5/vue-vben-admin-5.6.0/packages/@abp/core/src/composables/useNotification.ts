import { notification } from 'ant-design-vue';

import { useEventBus } from './useEventBus';

/**
 * 全局消息通知
 * 只处理：广播 + 私信（弹窗提醒）
 * 其他业务事件（数据刷新等）在各自页面订阅
 */
export function useNotification() {
  const { subscribe } = useEventBus();

  function register() {
    // 订阅私信
    subscribe('signalR:ReceiveTextMessage', (message: any) => {
      showNotification(message);
    });

    // 订阅广播
    subscribe('signalR:ReceiveBroadCastMessage', (message: any) => {
      showNotification(message);
    });
  }

  function showNotification(message: any) {
    const { title, content, messageLevel } = message;

    if (messageLevel === 10) {
      notification.warn({
        message: title,
        description: content,
        duration: 4,
      });
    } else if (messageLevel === 30) {
      notification.error({
        message: title,
        description: content,
        duration: 6,
      });
    } else {
      notification.info({
        message: title,
        description: content,
        duration: 4,
      });
    }
  }

  return {
    register,
  };
}
