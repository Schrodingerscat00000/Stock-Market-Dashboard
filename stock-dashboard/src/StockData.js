import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
  TimeScale,
} from 'chart.js';
import 'chartjs-adapter-moment';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler, TimeScale);

const StockData = ({ ticker }) => {
  const [stockData, setStockData] = useState([]);
  const [movingAverages, setMovingAverages] = useState([]);
  const [highPrices, setHighPrices] = useState([]);
  const [lowPrices, setLowPrices] = useState([]);
  const [volumes, setVolumes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    axios.get(`https://localhost:5001/api/stock/${ticker}`)
      .then(response => {
        const { stockData, movingAverages, highPrices, lowPrices, volumes } = response.data;

        if (stockData) {
          setStockData(stockData);
          setMovingAverages(movingAverages);
          setHighPrices(highPrices);
          setLowPrices(lowPrices);
          setVolumes(volumes);
        } else {
          setError('Stock data is unavailable.');
        }
        setLoading(false);
      })
      .catch(error => {
        console.error('Error fetching stock data:', error);
        setError('Failed to fetch stock data.');
        setLoading(false);
      });
  }, [ticker]);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>{error}</p>;

console.log("Stock Data:", stockData);
console.log("Moving Averages:", movingAverages);
//console.log("Open Prices:", openPrices);
console.log("High Prices:", highPrices);
console.log("Low Prices:", lowPrices);
console.log("Volumes:", volumes);

  if (!stockData || stockData.length === 0) {
    return <p>No stock data available.</p>;
  }

  const dates = stockData.map(item => item?.Date);
  const closePrices = stockData.map(item => parseFloat(item?.Close));
  const smaData = movingAverages.map(value => (value !== null ? value : null));
  const highData = highPrices.map(price => parseFloat(price));
  const lowData = lowPrices.map(price => parseFloat(price));

  const data = {
    labels: dates.reverse(),
    datasets: [
      {
        label: 'Close Price',
        data: closePrices.reverse(),
        borderColor: 'rgba(75,192,192,1)',
        backgroundColor: 'rgba(75,192,192,0.2)',
        fill: true,
        tension: 0.4,
      },
      {
        label: '5-Day Moving Average',
        data: smaData.reverse(),
        borderColor: 'rgba(153,102,255,1)',
        backgroundColor: 'rgba(153,102,255,0.2)',
        fill: true,
        tension: 0.4,
      },
      {
        label: 'High Price',
        data: highData.reverse(),
        borderColor: 'rgba(255,99,132,1)',
        borderDash: [5, 5],
        fill: false,
      },
      {
        label: 'Low Price',
        data: lowData.reverse(),
        borderColor: 'rgba(54,162,235,1)',
        borderDash: [5, 5],
        fill: false,
      },
    ],
  };

  const options = {
    responsive: true,
    scales: {
      x: {
        type: 'time',
        time: {
          unit: 'day',
        },
        ticks: {
          autoSkip: true,
          maxTicksLimit: 10,
        },
        title: {
          display: true,
          text: 'Date',
          font: {
            size: 14,
          },
        },
        grid: {
          display: false,
        },
      },
      y: {
        grid: {
          color: 'rgba(200, 200, 200, 0.2)',
        },
        title: {
          display: true,
          text: 'Price (USD)',
          font: {
            size: 14,
          },
        },
      },
    },
    plugins: {
      legend: {
        position: 'top',
        labels: {
          font: {
            size: 14,
          },
        },
      },
      tooltip: {
        mode: 'index',
        intersect: false,
        callbacks: {
          label: function (tooltipItem) {
            return `${tooltipItem.dataset.label}: $${tooltipItem.raw.toFixed(2)}`;
          },
        },
      },
      title: {
        display: true,
        text: `Stock Prices, Highs, Lows, and Moving Averages for ${ticker}`,
        font: {
          size: 18,
        },
      },
    },
  };

  return (
    <div>
      <h2>Stock Prices and Summary for {ticker}</h2>
      <div className="summary">
        <p><strong>Most Recent Close Price:</strong> ${closePrices[closePrices.length - 1]}</p>
        <p><strong>Highest Price:</strong> ${Math.max(...highData)}</p>
        <p><strong>Lowest Price:</strong> ${Math.min(...lowData)}</p>
        <p><strong>Most Recent Volume:</strong> {volumes[volumes.length - 1]}</p>
      </div>
      <Line data={data} options={options} />
    </div>
  );
};

export default StockData;
