const WEATHER_API_URL = 'https://uapis.cn/api/v1/misc/weather';
const WEATHER_API_KEY = import.meta.env.VITE_UAPI_API_KEY as string | undefined;
const WEATHER_CACHE_TTL = 60 * 60 * 1000;

interface WeatherAlert {
  /** 防御指引列表 */
  guidance?: string[];
  /** 预警级别，如蓝色、黄色、橙色、红色 */
  level?: string;
  /** 预警发布时间 */
  publish_time?: string;
  /** 发布单位 */
  publisher?: string;
  /** 预警正文 */
  text?: string;
  /** 预警标题 */
  title?: string;
  /** 预警类型，如雷电、暴雨 */
  type?: string;
}

interface WeatherForecastDay {
  /** 日期 YYYY-MM-DD */
  date: string;
  /** 湿度 % */
  humidity?: number;
  /** 降水量 mm */
  precip?: number;
  /** 日出时间 HH:MM */
  sunrise?: string;
  /** 日落时间 HH:MM */
  sunset?: string;
  /** 最高温度 °C */
  temp_max?: number;
  /** 最低温度 °C */
  temp_min?: number;
  /** 紫外线指数 */
  uv_index?: number;
  /** 能见度 km */
  visibility?: number;
  /** 白天天气 */
  weather_day?: string;
  /** 夜间天气 */
  weather_night?: string;
  /** 星期几 */
  week?: string;
  /** 白天风向 */
  wind_dir_day?: string;
  /** 夜间风向 */
  wind_dir_night?: string;
  /** 白天风力 */
  wind_scale_day?: string;
  /** 夜间风力 */
  wind_scale_night?: string;
  /** 白天风速 km/h */
  wind_speed_day?: number;
}

interface WeatherHourlyItem {
  /** 体感温度 °C */
  feels_like?: number;
  /** 湿度 % */
  humidity?: number;
  /** 降水概率 % */
  pop?: number;
  /** 降水量 mm */
  precip?: number;
  /** 温度 °C */
  temperature?: number;
  /** 预报时间 */
  time: string;
  /** 紫外线指数 */
  uv_index?: number;
  /** 能见度 km */
  visibility?: number;
  /** 天气状况 */
  weather?: string;
  /** 风向 */
  wind_direction?: string;
  /** 风力等级 */
  wind_scale?: string;
  /** 风速 km/h */
  wind_speed?: number;
}

interface WeatherAirPollutants {
  /** 一氧化碳 mg/m³ */
  co?: number;
  /** 二氧化氮 μg/m³ */
  no2?: number;
  /** 臭氧 μg/m³ */
  o3?: number;
  /** PM10 μg/m³ */
  pm10?: number;
  /** PM2.5 μg/m³ */
  pm25?: number;
  /** 二氧化硫 μg/m³ */
  so2?: number;
}

interface WeatherLifeIndex {
  /** 详细建议 */
  advice?: string;
  /** 简短描述 */
  brief?: string;
  /** 等级名称 */
  level?: string;
}

interface WeatherMinutelyPrecip {
  /** 精确到 2 分钟的降水数据点 */
  data?: WeatherMinutelyPrecipItem[];
  /** 降水描述 */
  summary?: string;
  /** 更新时间 */
  update_time?: string;
}

interface WeatherMinutelyPrecipItem {
  /** 该时间点的降水量 mm */
  precip?: number;
  /** 预报时间 ISO8601 */
  time?: string;
  /** 降水类型：rain / snow */
  type?: 'rain' | 'snow';
}

interface WeatherData {
  /** 行政区划代码 */
  adcode?: string;
  /** 空气污染物分项数据 */
  air_pollutants?: WeatherAirPollutants;
  /** 官方气象预警列表 */
  alerts?: WeatherAlert[];
  /** 空气质量指数 0-500 */
  aqi?: number;
  /** AQI 等级描述，如优、良 */
  aqi_category?: string;
  /** AQI 等级 1-6 */
  aqi_level?: number;
  /** 主要污染物，如 PM2.5、PM10、O3 */
  aqi_primary?: string;
  /** 城市名 */
  city?: string;
  /** 云量 % */
  cloud?: number;
  /** 区县或更细一级的行政区名称 */
  district?: string;
  /** 体感温度 °C */
  feels_like?: number;
  /** 多天天气预报，最多 7 天 */
  forecast?: WeatherForecastDay[];
  /** 逐小时预报，最多 24 小时 */
  hourly_forecast?: WeatherHourlyItem[];
  /** 相对湿度 % */
  humidity?: number;
  /** 生活指数 */
  life_indices?: Record<string, WeatherLifeIndex>;
  /** 分钟级降水预报 */
  minutely_precip?: WeatherMinutelyPrecip;
  /** 当前降水量 mm */
  precipitation?: number;
  /** 气压 hPa */
  pressure?: number;
  /** 省份 */
  province?: string;
  /** 数据更新时间 */
  report_time?: string;
  /** 当天最高温 °C */
  temp_max?: number;
  /** 当天最低温 °C */
  temp_min?: number;
  /** 当前温度 °C */
  temperature?: number;
  /** 紫外线指数 */
  uv?: number;
  /** 能见度 km */
  visibility?: number;
  /** 天气状况描述 */
  weather?: string;
  /** 天气图标代码 */
  weather_icon?: string;
  /** 风向 */
  wind_direction?: string;
  /** 风力等级 */
  wind_power?: string;
}

const weatherCache = new Map<
  string,
  {
    data: WeatherData;
    expiresAt: number;
  }
>();

async function getWeather(city?: string, force = false): Promise<WeatherData> {
  const cacheKey = city || 'auto';
  const cached = weatherCache.get(cacheKey);
  if (!force && cached && cached.expiresAt > Date.now()) {
    return cached.data;
  }

  const params = new URLSearchParams({
    extended: 'true',
    forecast: 'true',
    lang: 'zh',
  });
  if (city) {
    params.set('city', city);
  }

  const headers: Record<string, string> = {};
  if (WEATHER_API_KEY) {
    headers.Authorization = `Bearer ${WEATHER_API_KEY}`;
  }

  const response = await fetch(`${WEATHER_API_URL}?${params.toString()}`, {
    headers,
  });
  if (!response.ok) {
    throw new Error(`Weather request failed: ${response.status}`);
  }
  const data = (await response.json()) as WeatherData;
  weatherCache.set(cacheKey, {
    data,
    expiresAt: Date.now() + WEATHER_CACHE_TTL,
  });
  return data;
}

export { getWeather };

export type {
  WeatherAirPollutants,
  WeatherAlert,
  WeatherData,
  WeatherForecastDay,
  WeatherHourlyItem,
  WeatherLifeIndex,
  WeatherMinutelyPrecip,
  WeatherMinutelyPrecipItem,
};
