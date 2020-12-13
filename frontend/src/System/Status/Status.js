import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import HealthConnector from './Health/HealthConnector';
import AboutConnector from './About/AboutConnector';
import MoreInfo from './MoreInfo/MoreInfo';

class Status extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title="Status">
        <PageContentBody>
          <HealthConnector />
          <AboutConnector />
          <MoreInfo />
        </PageContentBody>
      </PageContent>
    );
  }

}

export default Status;
