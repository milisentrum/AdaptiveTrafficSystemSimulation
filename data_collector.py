import zmq
import io
import json
from PIL import Image
from ultralytics import YOLO

# Условная модель
model_path = "yolov8m.pt"
model = YOLO(model_path)
model.to("cuda")  # или .to("cpu")

print(f"Loaded model: {model_path}")

# Определим классы для person / transport
PERSON_CLASS_IDS = {0}
TRANSPORT_CLASS_IDS = {2, 3, 5, 6, 7}

def main():
    # Создаём сокет REP
    context = zmq.Context()
    rep_socket = context.socket(zmq.REP)
    rep_socket.bind("tcp://0.0.0.0:5555")  # слушаем порт 5555

    print("REQ/REP сервер запущен на tcp://0.0.0.0:5555")

    while True:
        try:
            # Читаем multi-part запрос: ждем 2 фрейма
            #  1) wanted_str (напр. "person" или "transport")
            #  2) байты картинки
            frames = rep_socket.recv_multipart()
            if len(frames) < 2:
                # Если меньше 2 фреймов, пропускаем
                rep_socket.send_string("ERROR: not enough frames")
                continue

            wanted_str = frames[0].decode('utf-8')
            image_bytes = frames[1]

            # Декодируем в PIL Image
            img = Image.open(io.BytesIO(image_bytes))

            # Выполняем детекцию
            results = model(img)

            # Определяем нужные class_ids
            if wanted_str == "person":
                wanted_ids = PERSON_CLASS_IDS
            elif wanted_str == "transport":
                wanted_ids = TRANSPORT_CLASS_IDS
            else:
                wanted_ids = None  # Если что-то иное - обрабатывайте по-своему

            count = 0
            if wanted_ids is not None:
                for box in results[0].boxes:
                    cls = int(box.cls[0])
                    conf = float(box.conf[0])
                    if cls in wanted_ids and conf >= 0.3:
                        count += 1

            # Формируем JSON ответ
            resp_json = json.dumps({"count": count})
            print(f"Requested: {wanted_str}, count={count}")

            # Отправляем ответ (одним фреймом)
            rep_socket.send_string(resp_json)

        except Exception as e:
            err_str = f"ERROR: {e}"
            print(err_str)
            # Пробуем отослать ошибку
            rep_socket.send_string(err_str)

if __name__ == "__main__":
    main()