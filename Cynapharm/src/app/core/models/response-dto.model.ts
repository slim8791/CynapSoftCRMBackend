export interface ResponseDto<T = any> {
  result:    T | null;
  isSuccess: boolean;
  message:   string;
}
