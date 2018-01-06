import React from 'react';
import ReactDOM from 'react-dom';



class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            media: []
        };
    }
    componentDidMount() {
        // fetch('/api/bears')
        //     .then(resp => resp.json())
        //     .then(console.log);
    }
    render() {
        return <span>Hello world!</span>;
    }
}


ReactDOM.render(
    <App />,
    document.getElementById('app')
);