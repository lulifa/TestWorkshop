import type {
  IEnablePaging,
  PagedAndSortedResultRequestDto,
  RouteDto,
} from '../..';

interface LayoutDto extends RouteDto {
  dataId: string;
  framework: string;
}

interface LayoutCreateOrUpdateDto {
  description?: string;
  displayName: string;
  name: string;
  path: string;
  redirect?: string;
}

interface LayoutCreateDto extends LayoutCreateOrUpdateDto {
  dataId: string;
  framework: string;
}

type LayoutUpdateDto = LayoutCreateOrUpdateDto;

interface LayoutGetPagedListInput
  extends IEnablePaging, PagedAndSortedResultRequestDto {
  filter?: string;
  framework?: string;
}

export type {
  LayoutCreateDto,
  LayoutDto,
  LayoutGetPagedListInput,
  LayoutUpdateDto,
};
