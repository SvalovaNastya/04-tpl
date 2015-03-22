# Решатель японских кроссвордов и TPL

Необходимо написать программу, решающую японские кроссворды 
(для тех, кто не знает, что это такое — читайте [википедию](http://en.wikipedia.org/wiki/Nonogram). 
 
Последовательный алгоритм решения посмотреть [здесь](http://algolist.manual.ru/misc/japancross.php). 
Вам нужно нужно реализовать итерированный анализ линий из статьи без переборной составляющей.
Затем распараллелить реализацию алгоритма за счет TPL и сравнить скорость работы.

За переборную составляющую рекомендуется браться только после распараллеливания.
 
Модули в программе должны быть понятным образом декомпозированы, 
важные части кода покрыты тестами. 
Часть тестов мы уже написали для вас, они в репозитории. 
Вам остаётся сделать две реализации интерфейса ICrosswordSolver: одно- и многопоточную. 
Обе должны проходить все тесты.

```
public interface ICrosswordSolver
{
                SolutionStatus Solve(string inputFilePath, string outputFilePath);
}
 
public enum SolutionStatus
{
                /// <description>
                /// Путь до исходного файла некорректен (невалидный путь, нет доступа, исходный файл не существует или произошла неизвестная ошибка в процессе чтения)
                /// </description>
                BadInputFilePath,
 
                /// <description>
                /// Путь до выходного файла некорректен (невалидный путь, нет доступа или произошла неизвестная ошибка в процессе записи)
                /// </description>
                BadOutputFilePath,
 
                /// <description>
                /// Исходный кроссворд некорректен
                /// </description>
                IncorrectCrossword,
 
                /// <description>
                /// Кроссворд решён частично
                /// </description>
                PartiallySolved,
 
                /// <description>
                /// Кроссворд решён полностью
                /// </description>
                Solved
}
```