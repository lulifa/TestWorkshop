import { requestClient } from '@abp/core';

const DEFAULT_AVATAR_OWNER_TYPE = 'DefaultAvatar';

/**
 * 获取系统级默认头像文件。
 * 未配置时接口会抛错，由调用方决定回退头像。
 */
export async function getDefaultAvatarApi(): Promise<Blob | undefined> {
  const picture = await requestClient.download<Blob>(
    '/api/platform/files/by-owner',
    {
      params: {
        ownerType: DEFAULT_AVATAR_OWNER_TYPE,
      },
    },
  );
  return picture.size > 0 ? picture : undefined;
}
