import React from 'react';
import StockData from './StockData';

function App() {
  return (
    <div className="App">
      <h1>Stock Market Dashboard</h1>
      {/* Pass the stock ticker as a prop */}
      <StockData ticker="AAPL" />
    </div>
  );
}

export default App;
