import type { IEnablePaging, PagedResultRequestDto } from '../..';

/** 文件列表查询输入 */
interface GetFileListInput extends IEnablePaging, PagedResultRequestDto {
  /** 结束时间 */
  endTime?: Date;
  /** 关键字 */
  keyword?: string;
  /** 业务ID */
  ownerId?: string;
  /** 业务类型 */
  ownerType?: string;
  /** 开始时间 */
  startTime?: Date;
}

/** 文件对象 DTO */
interface FileObjectDto {
  /** 文件类型（MIME） */
  contentType?: string;
  /** 上传时间 */
  creationTime: Date;
  /** 文件名 */
  fileName: string;
  /** 文件大小（字节） */
  fileSize: number;
  /** 文件大小（格式化显示） */
  fileSizeText: string;
  /** 文件ID */
  id: string;
  /** 业务ID */
  ownerId?: string;
  /** 业务类型 */
  ownerType?: string;
}

export type { FileObjectDto, GetFileListInput };
