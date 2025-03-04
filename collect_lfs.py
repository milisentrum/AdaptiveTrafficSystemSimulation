import os
import glob
import shutil
import zipfile
import concurrent.futures

# Имя файла с правилами для Git LFS
GITATTR_FILE = ".gitattributes"
# Папка, куда будут скопированы LFS-файлы
OUTPUT_DIR = "_lfs_files"
# Имя итогового ZIP-архива
ARCHIVE_NAME = "lfs_files_archive.zip"

def print_progress_bar(iteration, total, prefix='', suffix='', length=50):
    """
    Печатает простой ASCII-прогресс-бар в одну строку.

    :param iteration: Текущее значение (количество обработанных единиц).
    :param total: Общее количество.
    :param prefix: Текст перед прогресс-баром.
    :param suffix: Текст после прогресс-бара (например, 'Complete').
    :param length: Ширина прогресс-бара в символах.
    """
    # Защита от деления на ноль и случая, когда total = 0
    if total <= 0:
        total = 1

    percent = 100 * (iteration / float(total))
    filled_length = int(length * iteration // total)
    bar = '█' * filled_length + '-' * (length - filled_length)
    # \r - возврат каретки, чтобы перезаписывать строку
    print(f'\r{prefix} |{bar}| {percent:6.2f}% {suffix}', end='')
    if iteration >= total:
        print()  # Перевод строки после завершения

def read_lfs_patterns(gitattributes_path):
    """
    Считывает все шаблоны (*.png, *.fbx и т.д.), для которых прописан 'filter=lfs' в .gitattributes.
    Возвращает список таких шаблонов.
    """
    patterns = []
    if not os.path.exists(gitattributes_path):
        return patterns
    
    with open(gitattributes_path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            # Игнорируем пустые и комментированные строки
            if not line or line.startswith("#"):
                continue
            # Проверяем, что есть 'filter=lfs'
            if "filter=lfs" in line:
                # Первый элемент строки до пробела - это шаблон (например, '*.png')
                parts = line.split()
                pattern = parts[0].replace('"', '')
                patterns.append(pattern)
    return patterns

def find_all_files_by_patterns(patterns, root_dir="."):
    """
    Рекурсивно ищет все файлы по списку шаблонов patterns,
    начиная с root_dir. Возвращает список путей к найденным файлам.
    """
    matched_files = []
    for pattern in patterns:
        search_pattern = os.path.join(root_dir, "**", pattern)
        found = glob.glob(search_pattern, recursive=True)
        matched_files.extend(found)
    return matched_files

def prepare_directories(files, src_root, dst_root):
    """
    Создаёт структуру директорий в dst_root для всех файлов из списка,
    чтобы не было коллизий при параллельном копировании.
    """
    for file_path in files:
        rel_path = os.path.relpath(file_path, src_root)
        dest_path = os.path.join(dst_root, rel_path)
        os.makedirs(os.path.dirname(dest_path), exist_ok=True)

def copy_single_file(args):
    """
    Копирует один файл (src -> dst).
    Возвращает (src, dst) для статистики или отображения.
    """
    src, dst = args
    shutil.copy2(src, dst)
    return (src, dst)

def copy_files_parallel(files, src_root, dst_root, max_workers=8):
    """
    Копирует все файлы из src_root в dst_root параллельно (в max_workers потоках),
    используя ASCII-прогресс-бар для наглядности.

    :param files: Список исходных путей к файлам.
    :param src_root: Корневой каталог (откуда копируем).
    :param dst_root: Папка, куда копируем.
    :param max_workers: Кол-во потоков для ThreadPoolExecutor.
    """
    # Формируем список (src_path, dst_path)
    file_pairs = []
    for file_path in files:
        rel_path = os.path.relpath(file_path, src_root)
        dest_path = os.path.join(dst_root, rel_path)
        file_pairs.append((file_path, dest_path))

    total = len(file_pairs)
    if total == 0:
        print("Нет файлов для копирования.")
        return

    print(f"Начинаем копирование {total} файлов в {max_workers} поток(-а/-ов).")

    copied_count = 0
    # Запускаем пул потоков
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
        future_to_file = {executor.submit(copy_single_file, pair): pair for pair in file_pairs}
        
        # По мере завершения задач, обновляем прогресс-бар
        for future in concurrent.futures.as_completed(future_to_file):
            _pair = future_to_file[future]
            try:
                future.result()  # если нужно, можно вернуть (src, dst)
            except Exception as e:
                print(f"\nОшибка при копировании {_pair}: {e}")
            copied_count += 1
            print_progress_bar(copied_count, total, prefix='Копирование', suffix='файлов')

    print("Копирование файлов завершено!\n")

def zip_directory(src_dir, zip_path):
    """
    Упаковывает содержимое папки src_dir в ZIP-архив zip_path.
    Для наглядности показывает прогресс-бар по количеству файлов.

    :param src_dir: Папка, чьё содержимое архивируем.
    :param zip_path: Путь к создаваемому ZIP-файлу.
    """
    # Собираем список всех файлов, чтобы знать общее количество
    all_files = []
    for root, dirs, files in os.walk(src_dir):
        for f in files:
            full_path = os.path.join(root, f)
            all_files.append(full_path)

    total_files = len(all_files)
    if total_files == 0:
        print(f"Папка '{src_dir}' пуста. Архив не создаётся.")
        return

    print(f"Архивируем {total_files} файл(-ов) в '{zip_path}'...")

    zipped_count = 0
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zipf:
        for file_path in all_files:
            rel_path = os.path.relpath(file_path, src_dir)
            zipf.write(file_path, rel_path)
            zipped_count += 1
            print_progress_bar(zipped_count, total_files, prefix='Архивирование', suffix='файлов')

    print("Архивирование завершено!\n")

def main():
    print("Чтение LFS-паттернов из .gitattributes...")
    lfs_patterns = read_lfs_patterns(GITATTR_FILE)
    if not lfs_patterns:
        print("Не найдены LFS-паттерны (filter=lfs) в .gitattributes или файл отсутствует.\n")
        return
    print(f"Найдены LFS-паттерны: {lfs_patterns}\n")

    print("Поиск файлов, соответствующих LFS-паттернам...")
    all_lfs_files = find_all_files_by_patterns(lfs_patterns, root_dir=".")
    print(f"Всего найдено {len(all_lfs_files)} файлов, подпадающих под LFS-паттерны.\n")

    # Если хотим «обнулить» предыдущие результаты
    if os.path.exists(OUTPUT_DIR):
        shutil.rmtree(OUTPUT_DIR)
        print(f"Старая папка '{OUTPUT_DIR}' удалена.\n")
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    print(f"Создаём структуру папок в '{OUTPUT_DIR}'...")
    prepare_directories(all_lfs_files, ".", OUTPUT_DIR)
    print("Структура папок подготовлена.\n")

    print("Начинаем копирование файлов...")
    copy_files_parallel(all_lfs_files, ".", OUTPUT_DIR, max_workers=8)

    print("Приступаем к созданию ZIP-архива...")
    zip_directory(OUTPUT_DIR, ARCHIVE_NAME)

    print(f"Готово! Папка '{OUTPUT_DIR}' и архив '{ARCHIVE_NAME}' созданы.")
    print("Теперь их можно добавить в .gitignore и передавать вручную.\n")

if __name__ == "__main__":
    main()
