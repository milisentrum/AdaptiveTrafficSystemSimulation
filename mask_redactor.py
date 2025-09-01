"""
Утилита на tkinter для интерактивного создания маски на изображении.

Возможности:
1. Загрузка изображения.
2. Отмечаем точки на изображении кликами, формируем полигон.
3. Рисуем замкнутый полигон поверх исходного изображения.
4. Просмотр результата наложения маски (в отдельном окне).
5. Сохранение маски в PNG.

Запуск:
python MaskUtility_Tk.py

Установка зависимостей:
pip install pillow
"""

import tkinter as tk
from tkinter import filedialog
from PIL import Image, ImageTk, ImageDraw
import os

class MaskUtilityTk:
    def __init__(self, master):
        self.master = master
        self.master.title("Mask Utility TK")

        # Изначально нет изображения
        self.original_image = None
        self.tk_image = None
        self.points = []
        self.canvas = None
        self.image_on_canvas = None
        self.image_width = 0
        self.image_height = 0

        # Верхняя панель с кнопками
        btn_frame = tk.Frame(self.master)
        btn_frame.pack(side=tk.TOP, pady=5)

        load_btn = tk.Button(btn_frame, text="Загрузить изображение", command=self.load_image)
        load_btn.pack(side=tk.LEFT, padx=5)

        show_mask_btn = tk.Button(btn_frame, text="Показать маску", command=self.show_mask)
        show_mask_btn.pack(side=tk.LEFT, padx=5)

        save_mask_btn = tk.Button(btn_frame, text="Сохранить маску", command=self.save_mask)
        save_mask_btn.pack(side=tk.LEFT, padx=5)

        clear_btn = tk.Button(btn_frame, text="Очистить точки", command=self.clear_points)
        clear_btn.pack(side=tk.LEFT, padx=5)

    def load_image(self):
        filepath = filedialog.askopenfilename(
            title="Выберите изображение",
            filetypes=[("Image Files", "*.png;*.jpg;*.jpeg;*.webp;*.tif;*.tiff"),('WEBP', '*.webp')])
        if not filepath:
            return

        self.original_image = Image.open(filepath).convert("RGB")
        self.points = []  # Сбрасываем точки
        self.show_image()

    def show_image(self):
        if self.original_image is None:
            return

        # Если canvas ещё не создан, создаём
        if self.canvas is None:
            self.canvas = tk.Canvas(self.master, cursor="tcross")
            self.canvas.pack(side=tk.TOP, fill=tk.BOTH, expand=True)
            self.canvas.bind("<Button-1>", self.on_canvas_click)

        # Масштабируем картинку под размер окна (по желанию)
        # Можно оставить 1:1, если изображение не слишком большое.
        self.image_width, self.image_height = self.original_image.size

        self.tk_image = ImageTk.PhotoImage(self.original_image)
        self.canvas.config(width=self.image_width, height=self.image_height)
        self.image_on_canvas = self.canvas.create_image(0, 0, anchor=tk.NW, image=self.tk_image)

    def on_canvas_click(self, event):
        # Добавляем точку в список
        x, y = event.x, event.y
        self.points.append((x, y))
        # Перерисовываем полигон
        self.draw_polygon()

    def draw_polygon(self):
        # Сначала удаляем предыдущие рисунки (кроме изображения)
        self.canvas.delete("polygon_line")  # Удалим все объекты с тегом "polygon_line"

        # Рисуем линии между точками
        if len(self.points) > 1:
            # Формируем список координат
            coords = []
            for (x, y) in self.points:
                coords.append(x)
                coords.append(y)
            # Замыкаем полигон
            coords.append(self.points[0][0])
            coords.append(self.points[0][1])
            self.canvas.create_line(coords, fill="red", width=2, tags="polygon_line")

        # Рисуем точки
        for (x, y) in self.points:
            r = 3
            self.canvas.create_oval(x-r, y-r, x+r, y+r, fill="red", outline="red", tags="polygon_line")

    def get_masked_image(self):
        # Создаем маску
        if self.original_image is None:
            return None, None
        mask = Image.new("L", self.original_image.size, 0)
        draw = ImageDraw.Draw(mask)
        if len(self.points) > 2:
            draw.polygon(self.points, outline=255, fill=255)

        # Применяем маску
        masked_image = Image.composite(
            self.original_image,
            Image.new("RGB", self.original_image.size, (0, 0, 0)),
            mask
        )
        return masked_image, mask

    def show_mask(self):
        masked_image, mask = self.get_masked_image()
        if masked_image is None:
            return

        # Открываем новое окно для показа результата
        top = tk.Toplevel(self.master)
        top.title("Маскированное изображение")

        # Маскированная картинка
        tk_masked = ImageTk.PhotoImage(masked_image)
        lbl_masked = tk.Label(top, image=tk_masked)
        lbl_masked.image = tk_masked  # чтобы не сборщик мусора
        lbl_masked.pack(side=tk.LEFT)

        # Сама маска (черно-белая)
        tk_mask_only = ImageTk.PhotoImage(mask)
        lbl_mask = tk.Label(top, image=tk_mask_only)
        lbl_mask.image = tk_mask_only
        lbl_mask.pack(side=tk.LEFT)

    def save_mask(self):
        # Сохраняем маску в файл
        masked_image, mask = self.get_masked_image()
        if mask is None:
            return
        filepath = filedialog.asksaveasfilename(
            defaultextension=".png",
            filetypes=[("PNG", "*.png"), ("JPEG", "*.jpg"), ("All files", "*.*")]
        )
        if not filepath:
            return
        mask.save(filepath)
        print(f"Маска сохранена: {filepath}")

    def clear_points(self):
        self.points = []
        if self.canvas:
            self.canvas.delete("polygon_line")


def main():
    root = tk.Tk()
    app = MaskUtilityTk(root)
    root.mainloop()

if __name__ == "__main__":
    main()
