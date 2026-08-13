import { requestClient } from '@abp/core';

export const USER_AVATAR_OWNER_TYPE = 'User';

/** 获取当前用户头像，未上传时为空，由前端兜底静态图。 */
export async function getCurrentUserAvatarApi(): Promise<Blob | undefined> {
  const picture = await requestClient.download<Blob>(
    '/api/platform/files/user-avatar',
  );
  return picture.size > 0 ? picture : undefined;
}
