using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicLottery
{
    class Generator
    {
        private List<char> listReadyChars;
        private List<char> listNumbers = new List<char>();
        private List<char> listChars = new List<char>();
        private Random random = new Random();
        private int speed = 30;
        private int changeCount = 100;

        public Generator(List<char> listCharsInput, List<char> listNumbersInput)
        {
            listChars.AddRange(listCharsInput);
            listNumbers.AddRange(listNumbersInput);
        }
        /// <summary>
        /// Тоог эргэлдүүлэх
        /// </summary>
        public void Roller()
        {
            int index = 0;
            // Солигдох тоо
            for (int i = 0; i < changeCount; i++)
            {
                index = random.Next(0, listNumbers.Count);
                OnNextEvent(listNumbers[index]);
                System.Threading.Thread.Sleep(speed);
            }
            // Боломжит тоонуудаас санамсаргүйгээр сонгох
            index = random.Next(0, listReadyChars.Count);
            // Үндсэн дэлгэцрүү дууссан мэдээллийг дамжуулах
            OnFinishEvent(listReadyChars[index]);
        }
        /// <summary>
        /// Тоог эргэлдүүлэх
        /// </summary>
        public void RollerChar()
        {
            int index = 0;
            // Солигдох тоо
            for (int i = 0; i < changeCount; i++)
            {
                index = random.Next(0, listChars.Count);
                OnNextEvent(listChars[index]);
                System.Threading.Thread.Sleep(speed);
            }
            // Боломжит тоонуудаас санамсаргүйгээр сонгох
            index = random.Next(0, listReadyChars.Count);
            // Үндсэн дэлгэцрүү дууссан мэдээллийг дамжуулах
            OnFinishEvent(listReadyChars[index]);
        }
        /// <summary>
        /// Боломжит цифрийг оноох
        /// </summary>
        /// <param name="listReadyChars"></param>
        public void SetReadyChars(List<char> listReadyChars)
        {
            this.listReadyChars = ShuffleList(listReadyChars);
        }
        /// <summary>
        /// Шалгаруулалт дууссан
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="result"></param>
        public delegate void FinishEventHandler(object sender, char result);
        public event FinishEventHandler FinishEvent;
        public virtual void OnFinishEvent(char result)
        {
            if (FinishEvent != null)
            {
                FinishEvent(this, result);
            }
        }
        /// <summary>
        /// Дараагийн дугаар шалгаруулах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="result"></param>
        public delegate void NextEventHandler(object sender, char result);
        public event NextEventHandler NextEvent;
        public virtual void OnNextEvent(char result)
        {
            if (NextEvent != null)
            {
                NextEvent(this, result);
            }
        }
        /// <summary>
        /// Санамсаргүйгээр сэлгэх
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="inputList"></param>
        /// <returns></returns>
        private List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = random.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }
        /// <summary>
        /// Цифр солигдох тоо, хурдыг тохируулах
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="changeCount"></param>
        public void SetSpeed(int speed, int changeCount)
        {
            this.speed = speed;
            this.changeCount = changeCount;
        }
    }
}
