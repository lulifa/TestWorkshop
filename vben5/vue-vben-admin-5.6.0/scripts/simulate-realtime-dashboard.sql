-- Simulate three workshops with five online devices each.
-- Run with: psql <connection-string> -f scripts/simulate-realtime-dashboard.sql

\encoding UTF8

INSERT INTO "AppWorkshopDevices"
    ("Id", "Code", "Name", "Type", "OrganizationUnitId", "TenantId",
     "ExtraProperties", "ConcurrencyStamp", "CreationTime", "IsDeleted")
VALUES
    ('3a23e343-7306-67e3-7171-97055130f7aa', 'HD-FIVA-001', 'FIVA测试台', 1,
     '3a23e341-f6ed-c532-6f66-c7df2c72cc31', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245101-0000-0000-0000-000000000101', 'HD-PUMP-002', '黄岛水泵测试台', 2,
     '3a23e341-f6ed-c532-6f66-c7df2c72cc31', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245102-0000-0000-0000-000000000102', 'HD-SENSOR-003', '黄岛传感器组', 4,
     '3a23e341-f6ed-c532-6f66-c7df2c72cc31', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a24503a-dd6c-f2ae-e894-96f39d149b9c', 'SIM-NB-SENSOR-133418', '宁波传感器', 4,
     '3a23e342-24be-80f4-d036-8aeb3cf29fb2', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245201-0000-0000-0000-000000000201', 'NB-FIVA-002', '宁波FIVA测试台', 1,
     '3a23e342-24be-80f4-d036-8aeb3cf29fb2', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245202-0000-0000-0000-000000000202', 'NB-ICU-003', '宁波ICU测试台', 3,
     '3a23e342-24be-80f4-d036-8aeb3cf29fb2', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245301-0000-0000-0000-000000000301', 'SZ-FIVA-001', '深圳FIVA测试台', 1,
     '3a246e45-f3d7-19a9-3d95-0750cf050e50', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245302-0000-0000-0000-000000000302', 'SZ-PUMP-002', '深圳水泵测试台', 2,
     '3a246e45-f3d7-19a9-3d95-0750cf050e50', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245303-0000-0000-0000-000000000303', 'SZ-SENSOR-003', '深圳传感器组', 4,
     '3a246e45-f3d7-19a9-3d95-0750cf050e50', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245103-0000-0000-0000-000000000103', 'HD-ICU-004', '黄岛ICU测试台', 3,
     '3a23e341-f6ed-c532-6f66-c7df2c72cc31', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245104-0000-0000-0000-000000000104', 'HD-FIVA-005', '黄岛FIVA二号', 1,
     '3a23e341-f6ed-c532-6f66-c7df2c72cc31', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245203-0000-0000-0000-000000000203', 'NB-PUMP-004', '宁波水泵测试台', 2,
     '3a23e342-24be-80f4-d036-8aeb3cf29fb2', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245204-0000-0000-0000-000000000204', 'NB-SENSOR-005', '宁波传感器二号', 4,
     '3a23e342-24be-80f4-d036-8aeb3cf29fb2', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245304-0000-0000-0000-000000000304', 'SZ-ICU-004', '深圳ICU测试台', 3,
     '3a246e45-f3d7-19a9-3d95-0750cf050e50', NULL, '{}', gen_random_uuid()::text, NOW(), false),
    ('3a245305-0000-0000-0000-000000000305', 'SZ-FIVA-005', '深圳FIVA二号', 1,
     '3a246e45-f3d7-19a9-3d95-0750cf050e50', NULL, '{}', gen_random_uuid()::text, NOW(), false)
ON CONFLICT ("Id") DO UPDATE SET
    "Code" = EXCLUDED."Code",
    "Name" = EXCLUDED."Name",
    "Type" = EXCLUDED."Type",
    "OrganizationUnitId" = EXCLUDED."OrganizationUnitId",
    "IsDeleted" = false;

-- Demo heartbeat times are intentionally set in the future so the dashboard
-- keeps showing all devices. This blocks real heartbeat accumulation until the
-- future time catches up. Reset LastHeartbeatAt to NOW() when testing runtime.
INSERT INTO "AppWorkshopDeviceHeartbeats"
    ("Id", "DeviceId", "DeviceCode", "LastHeartbeatAt", "FirstHeartbeatAt",
     "TotalRuntimeSeconds")
SELECT
    gen_random_uuid(),
    d."Id",
    d."Code",
    NOW() + INTERVAL '30 minutes',
    NOW() - make_interval(days => v.first_days),
    v.runtime_seconds
FROM (
    VALUES
        ('HD-FIVA-001', 2, 193380),
        ('HD-PUMP-002', 2, 96120),
        ('HD-SENSOR-003', 2, 54120),
        ('SIM-NB-SENSOR-133418', 1, 130680),
        ('NB-FIVA-002', 2, 172440),
        ('NB-ICU-003', 1, 78840),
        ('SZ-FIVA-001', 2, 210600),
        ('SZ-PUMP-002', 1, 113400),
        ('SZ-SENSOR-003', 1, 63720),
        ('HD-ICU-004', 1, 84960),
        ('HD-FIVA-005', 2, 145800),
        ('NB-PUMP-004', 1, 104400),
        ('NB-SENSOR-005', 1, 68760),
        ('SZ-ICU-004', 1, 93600),
        ('SZ-FIVA-005', 2, 162000)
) AS v(device_code, first_days, runtime_seconds)
JOIN "AppWorkshopDevices" d ON d."Code" = v.device_code
ON CONFLICT ("DeviceCode") DO UPDATE SET
    "LastHeartbeatAt" = EXCLUDED."LastHeartbeatAt",
    "FirstHeartbeatAt" = EXCLUDED."FirstHeartbeatAt",
    "TotalRuntimeSeconds" = EXCLUDED."TotalRuntimeSeconds";

INSERT INTO "AppWorkshopRealtimeData"
    ("DeviceId", "Timestamp", "DeviceCode", "Pressure", "Flow", "Temperature",
     "Torque", "Vibration", "EmergencyStop", "ReceivedAt")
SELECT
    d."Id",
    NOW() - make_interval(secs => v.age_seconds),
    d."Code",
    v.pressure,
    v.flow,
    v.temperature,
    v.torque,
    v.vibration,
    false,
    NOW()
FROM (
    VALUES
        ('HD-FIVA-001', 1.28::numeric, 36.7::numeric, 42.6::numeric, 18.4::numeric, 2.1::numeric, 3),
        ('HD-PUMP-002', 0.86::numeric, 24.5::numeric, 39.2::numeric, 12.8::numeric, 1.7::numeric, 6),
        ('HD-SENSOR-003', 0.31::numeric, 8.6::numeric, 26.4::numeric, NULL::numeric, 0.9::numeric, 9),
        ('SIM-NB-SENSOR-133418', 0.42::numeric, 12.4::numeric, 28.7::numeric, NULL::numeric, 0.8::numeric, 5),
        ('NB-FIVA-002', 1.16::numeric, 32.9::numeric, 40.8::numeric, 16.7::numeric, 1.9::numeric, 8),
        ('NB-ICU-003', 0.73::numeric, 20.8::numeric, 33.5::numeric, 10.5::numeric, 1.2::numeric, 11),
        ('SZ-FIVA-001', 1.34::numeric, 38.1::numeric, 44.2::numeric, 19.6::numeric, 2.3::numeric, 4),
        ('SZ-PUMP-002', 0.91::numeric, 26.3::numeric, 38.1::numeric, 13.6::numeric, 1.6::numeric, 7),
        ('SZ-SENSOR-003', 0.36::numeric, 9.8::numeric, 27.9::numeric, NULL::numeric, 1.0::numeric, 10),
        ('HD-ICU-004', 0.77::numeric, 21.6::numeric, 34.2::numeric, 11.9::numeric, 1.4::numeric, 12),
        ('HD-FIVA-005', 1.22::numeric, 35.4::numeric, 41.7::numeric, 17.5::numeric, 2.0::numeric, 13),
        ('NB-PUMP-004', 0.88::numeric, 25.1::numeric, 37.6::numeric, 13.1::numeric, 1.5::numeric, 14),
        ('NB-SENSOR-005', 0.45::numeric, 11.9::numeric, 29.1::numeric, NULL::numeric, 0.7::numeric, 15),
        ('SZ-ICU-004', 0.69::numeric, 19.4::numeric, 32.8::numeric, 9.8::numeric, 1.1::numeric, 16),
        ('SZ-FIVA-005', 1.31::numeric, 37.2::numeric, 43.1::numeric, 18.9::numeric, 2.2::numeric, 17)
) AS v(device_code, pressure, flow, temperature, torque, vibration, age_seconds)
JOIN "AppWorkshopDevices" d ON d."Code" = v.device_code
ON CONFLICT ("DeviceId", "Timestamp") DO UPDATE SET
    "Pressure" = EXCLUDED."Pressure",
    "Flow" = EXCLUDED."Flow",
    "Temperature" = EXCLUDED."Temperature",
    "Torque" = EXCLUDED."Torque",
    "Vibration" = EXCLUDED."Vibration",
    "EmergencyStop" = EXCLUDED."EmergencyStop",
    "ReceivedAt" = EXCLUDED."ReceivedAt";
