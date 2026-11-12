/** 固定业务角色，需与后端 RoleConstants 保持一致 */
export const RoleConstants = {
  /** 管理员 */
  Admin: 'admin',
  /** 质量审核员 */
  Auditor: 'auditor',
  /** 调度 */
  Dispatcher: 'dispatcher',
  /** 访客 */
  Guest: 'guest',
  /** 单船项目主管 */
  ProjectSupervisor: 'projectSupervisor',
  /** 大区负责人 */
  RegionalManager: 'regionalManager',
  /** 车间主管 */
  Supervisor: 'supervisor',
  /** 测试工程师 */
  Tester: 'tester',
} as const;

export type RoleName = (typeof RoleConstants)[keyof typeof RoleConstants];
